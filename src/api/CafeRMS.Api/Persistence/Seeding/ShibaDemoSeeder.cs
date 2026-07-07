using CafeRMS.Api.Features.Allergens;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events;
using CafeRMS.Api.Features.Loyalty;
using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Features.Modifiers;
using CafeRMS.Api.Features.Orders;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.PrintoutTemplates;
using CafeRMS.Api.Features.ProductLists;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.PromotionCodes;
using CafeRMS.Api.Features.SalesChannels;
using CafeRMS.Api.Features.Tables;
using CafeRMS.Api.Features.Tags;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Persistence.Seeding;

public class ShibaDemoSeeder(
    AppDbContext db,
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IWebHostEnvironment environment,
    ILogger<ShibaDemoSeeder> logger)
{
    // ── Demo login accounts ────────────────────────────────────────────────
    // Every account below logs in with the same password: Demo1234
    //   admin@shiba.pl     Owner     — Dariusz Luśnia
    //   manager@shiba.pl   Manager   — Maja Kowalska
    //   barista@shiba.pl   Barista   — Anna Nowak
    //   user1@shiba.pl     Customer  — Zofia Wiśniewska
    //   user2@shiba.pl     Customer  — Jakub Lewandowski
    // ────────────────────────────────────────────────────────────────────────
    private const string DemoPassword = "Demo1234";

    private const string OwnerEmail = "admin@shiba.pl";
    private const string ManagerEmail = "manager@shiba.pl";
    private const string BaristaEmail = "barista@shiba.pl";
    private const string Customer1Email = "user1@shiba.pl";
    private const string Customer2Email = "user2@shiba.pl";

    private const string OutletDisplayName = "Shiba Cafe Warszawa";
    private const string OutletAddress = "ul. Marszałkowska 100, 00-001 Warszawa";

    public async Task SeedAsync()
    {
        var isAlreadySeeded = await db.Outlets.AnyAsync(x => x.DisplayName == OutletDisplayName);
        if (isAlreadySeeded)
            return;

        var (ownerUserId, outletId) = await SeedOwnerAndOutletAsync(DemoPassword);

        var taxRates = await SeedTaxRatesAsync();
        var tags = await SeedTagsAsync();
        var allergens = await SeedAllergensAsync();
        var modifierGroups = await SeedModifierGroupsAsync();
        var priceGroups = await SeedPriceGroupsAsync();
        var salesChannels = await SeedSalesChannelsAsync(priceGroups);
        var tables = await SeedTablesAsync(outletId);
        var products = await SeedProductsAsync(taxRates, tags, allergens, modifierGroups, priceGroups);
        var productLists = await SeedProductListsAsync(products);

        var outlet = await db.Outlets.FirstAsync(x => x.Id == outletId);
        outlet.SetDefaultMenu(priceGroups.Standard.Id, productLists.StandardMenu.Id);
        await db.SaveChangesAsync();
        var staff = await SeedStaffAsync(DemoPassword);
        var customers = await SeedCustomersAsync(DemoPassword);
        await SeedLoyaltyAsync(customers);
        var events = await SeedEventsAsync(productLists, priceGroups);
        var promotions = await SeedPromotionCodesAsync();
        await SeedSampleOrdersAsync(outletId, products, customers, tables, salesChannels, promotions, events);
        await SeedPrintoutTemplatesAsync();

        await db.SaveChangesAsync();

        await BackdateAuditsForVarietyAsync([ownerUserId, staff.ManagerUserId, staff.BaristaUserId]);

        logger.LogInformation(
            "Seeded Shiba demo (OutletId={OutletId}, OwnerUserId={OwnerUserId})",
            outletId, ownerUserId);
    }

    private async Task<(Guid OwnerUserId, Guid OutletId)> SeedOwnerAndOutletAsync(string ownerPassword)
    {
        var ownerRole = await roleManager.FindByNameAsync(SystemRoles.Owner);
        if (ownerRole is null)
        {
            ownerRole = AppRole.Create(SystemRoles.Owner);
            var roleResult = await roleManager.CreateAsync(ownerRole);
            if (!roleResult.Succeeded)
                throw new DomainException(string.Join("; ", roleResult.Errors.Select(x => x.Description)));
        }

        var owner = AppUser.Create(OwnerEmail, "Dariusz", "Luśnia", AccountType.Staff);
        var createResult = await userManager.CreateAsync(owner, ownerPassword);
        if (!createResult.Succeeded)
            throw new DomainException(string.Join("; ", createResult.Errors.Select(x => x.Description)));

        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = owner.Id, RoleId = ownerRole.Id });
        await db.SaveChangesAsync();

        var outlet = Outlet.Create(
            OutletDisplayName,
            OutletAddress,
            "+48221234567",
            "Europe/Warsaw",
            Currency.PLN,
            legalName: "Shiba Cafe Sp. z o.o.",
            taxId: "5252781234",
            invoicingAddress: OutletAddress,
            billingEmail: "ksiegowosc@shiba.pl",
            billingPhone: "+48221234567");
        db.Outlets.Add(outlet);
        await db.SaveChangesAsync();

        return (owner.Id, outlet.Id);
    }

    public sealed record TaxRateRefs(TaxRate EatIn, TaxRate Takeaway, TaxRate Goods);

    private async Task<TaxRateRefs> SeedTaxRatesAsync()
    {
        var eatIn = TaxRate.Create("VAT 8%", "Gastronomia na miejscu", 8m);
        var takeaway = TaxRate.Create("VAT 5%", "Żywność na wynos", 5m);
        var goods = TaxRate.Create("VAT 23%", "Towary (kubki, akcesoria dla psów)", 23m);
        db.TaxRates.AddRange(eatIn, takeaway, goods);
        await db.SaveChangesAsync();
        return new TaxRateRefs(eatIn, takeaway, goods);
    }

    public sealed record TagRefs(Tag HotDrinks, Tag IcedDrinks, Tag Coffee, Tag Sweets, Tag Savoury, Tag Vegan);

    private async Task<TagRefs> SeedTagsAsync()
    {
        var hotDrinks = Tag.Create("Napoje gorące");
        var icedDrinks = Tag.Create("Napoje zimne");
        var coffee = Tag.Create("Kawa");
        var sweets = Tag.Create("Słodkie");
        var savoury = Tag.Create("Przekąski");
        var vegan = Tag.Create("Wegańskie");
        db.Tags.AddRange(hotDrinks, icedDrinks, coffee, sweets, savoury, vegan);
        await db.SaveChangesAsync();
        return new TagRefs(hotDrinks, icedDrinks, coffee, sweets, savoury, vegan);
    }

    public sealed record AllergenRefs(Allergen Milk, Allergen Egg, Allergen Gluten, Allergen Soy, Allergen Peanuts);

    private async Task<AllergenRefs> SeedAllergensAsync()
    {
        var milk = Allergen.Create("Mleko");
        var egg = Allergen.Create("Jajka");
        var gluten = Allergen.Create("Gluten");
        var soy = Allergen.Create("Soja");
        var peanuts = Allergen.Create("Orzeszki ziemne");
        db.Allergens.AddRange(milk, egg, gluten, soy, peanuts);
        await db.SaveChangesAsync();
        return new AllergenRefs(milk, egg, gluten, soy, peanuts);
    }

    public sealed record ModifierGroupRefs(ModifierGroup Size, ModifierGroup Milk, ModifierGroup Sweetness, ModifierGroup Temperature);

    private async Task<ModifierGroupRefs> SeedModifierGroupsAsync()
    {
        var size = ModifierGroup.Create("Rozmiar");
        var milk = ModifierGroup.Create("Mleko");
        var sweetness = ModifierGroup.Create("Słodkość");
        var temperature = ModifierGroup.Create("Temperatura");

        db.ModifierGroups.AddRange(size, milk, sweetness, temperature);
        await db.SaveChangesAsync();

        db.Modifiers.AddRange(
            Modifier.Create("Standard", size.Id),
            Modifier.Create("Duży", size.Id),

            Modifier.Create("Krowie", milk.Id),
            Modifier.Create("Owsiane", milk.Id),
            Modifier.Create("Sojowe", milk.Id),

            Modifier.Create("Mała", sweetness.Id),
            Modifier.Create("Standardowa", sweetness.Id),

            Modifier.Create("Gorące", temperature.Id),
            Modifier.Create("Mrożone", temperature.Id));

        await db.SaveChangesAsync();
        return new ModifierGroupRefs(size, milk, sweetness, temperature);
    }

    public sealed record PriceGroupRefs(PriceGroup Standard, PriceGroup Promo);

    private async Task<PriceGroupRefs> SeedPriceGroupsAsync()
    {
        var standard = PriceGroup.Create("Standard");
        var loyalty = PriceGroup.Create("Promocja");
        db.PriceGroups.AddRange(standard, loyalty);
        await db.SaveChangesAsync();
        return new PriceGroupRefs(standard, loyalty);
    }

    public sealed record SalesChannelRefs(SalesChannel DineIn, SalesChannel Takeout);

    private async Task<SalesChannelRefs> SeedSalesChannelsAsync(PriceGroupRefs priceGroups)
    {
        var dineIn = SalesChannel.Create("Na miejscu", isTakeout: false);
        var takeout = SalesChannel.Create("Na wynos", isTakeout: true);
        db.SalesChannels.AddRange(dineIn, takeout);
        await db.SaveChangesAsync();

        db.SalesChannelPriceGroups.AddRange(
            SalesChannelPriceGroup.Create(dineIn.Id, priceGroups.Standard.Id),
            SalesChannelPriceGroup.Create(dineIn.Id, priceGroups.Promo.Id),
            SalesChannelPriceGroup.Create(takeout.Id, priceGroups.Standard.Id),
            SalesChannelPriceGroup.Create(takeout.Id, priceGroups.Promo.Id));

        await db.SaveChangesAsync();
        return new SalesChannelRefs(dineIn, takeout);
    }

    public sealed record TableRefs(Table Bar, Table Window, Table Tatami, Table Patio);

    private async Task<TableRefs> SeedTablesAsync(Guid outletId)
    {
        var bar = Table.Create("Bar 1", outletId);
        var window = Table.Create("Okno 1", outletId);
        var tatami = Table.Create("Tatami", outletId);
        var patio = Table.Create("Ogródek 1", outletId);
        db.Tables.AddRange(bar, window, tatami, patio);
        await db.SaveChangesAsync();
        return new TableRefs(bar, window, tatami, patio);
    }

    public sealed record ProductRefs(
        Product MatchaLatte, Product HojichaLatte, Product Cappuccino,
        Product YuzuLemoniada,
        Product ShibaCookie, Product MochiDonut, Product Cheesecake,
        Product OnigiriSalmon, Product ShibaMug);

    private async Task<ProductRefs> SeedProductsAsync(
        TaxRateRefs taxRates,
        TagRefs tags,
        AllergenRefs allergens,
        ModifierGroupRefs modifierGroups,
        PriceGroupRefs priceGroups)
    {
        var matchaLatte = AddProduct("Matcha latte", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.HotDrinks, tags.Vegan],
            [allergens.Soy],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var hojichaLatte = AddProduct("Hojicha latte", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.HotDrinks],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var cappuccino = AddProduct("Cappuccino", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.HotDrinks, tags.Coffee],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness]);
        var yuzuLemoniada = AddProduct("Yuzu lemoniada", taxRates.EatIn.Id, 16.00m, priceGroups,
            [tags.IcedDrinks, tags.Vegan],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness]);
        var shibaCookie = AddProduct("Ciastko Shiba", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.Sweets],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            []);
        var mochiDonut = AddProduct("Mochi donut", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.Sweets],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            []);
        var cheesecake = AddProduct("Sernik japoński", taxRates.EatIn.Id, 22.00m, priceGroups,
            [tags.Sweets],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            []);
        var onigiriSalmon = AddProduct("Onigiri łosoś", taxRates.Takeaway.Id, 12.00m, priceGroups,
            [tags.Savoury],
            [allergens.Soy],
            []);

        var shibaMug = AddProduct("Kubek emaliowany Shiba", taxRates.Goods.Id, 65.00m, priceGroups, [], [], []);

        await db.SaveChangesAsync();

        var products = new ProductRefs(matchaLatte, hojichaLatte, cappuccino, yuzuLemoniada, shibaCookie, mochiDonut, cheesecake, onigiriSalmon, shibaMug);
        SeedProductImages(products);
        await db.SaveChangesAsync();
        return products;
    }

    private void SeedProductImages(ProductRefs products)
    {
        var imagesByProduct = new (Product Product, string FileName)[]
        {
            (products.MatchaLatte, "matcha-latte.jpg"),
            (products.HojichaLatte, "hojicha-latte.jpg"),
            (products.Cappuccino, "cappuccino.jpg"),
            (products.YuzuLemoniada, "yuzu-lemonade.jpg"),
            (products.ShibaCookie, "shiba-cookie.jpg"),
            (products.MochiDonut, "mochi-donut.jpg"),
            (products.Cheesecake, "japanese-cheesecake.jpg"),
            (products.OnigiriSalmon, "onigiri-salmon.jpg"),
            (products.ShibaMug, "shiba-mug.jpg"),
        };

        var sourceDirectory = Path.Combine(environment.ContentRootPath, "Persistence", "Seeding", "demo-images");
        var uploadsDirectory = Path.Combine(environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDirectory);

        foreach (var (product, fileName) in imagesByProduct)
        {
            File.Copy(Path.Combine(sourceDirectory, fileName), Path.Combine(uploadsDirectory, fileName), overwrite: true);
            db.ProductImages.Add(ProductImage.Create(product.Id, $"/uploads/{fileName}"));
        }
    }

    private Product AddProduct(
        string name,
        Guid taxRateId,
        decimal basePrice,
        PriceGroupRefs priceGroups,
        Tag[] productTags,
        Allergen[] productAllergens,
        ModifierGroup[] productModifierGroups)
    {
        var product = Product.Create(name, taxRateId);
        db.Products.Add(product);

        db.ProductPrices.AddRange(
            ProductPrice.Create(product.Id, priceGroups.Standard.Id, basePrice),
            ProductPrice.Create(product.Id, priceGroups.Promo.Id, Discount(basePrice, 0.10m)));

        foreach (var tag in productTags)
            db.ProductTags.Add(ProductTag.Create(product.Id, tag.Id));
        foreach (var allergen in productAllergens)
            db.ProductAllergens.Add(ProductAllergen.Create(product.Id, allergen.Id));
        foreach (var group in productModifierGroups)
            db.ProductModifierGroups.Add(ProductModifierGroup.Create(product.Id, group.Id));

        return product;
    }

    private static decimal Discount(decimal net, decimal percentage) =>
        Math.Round(net * (1m - percentage), 2);

    public sealed record ProductListRefs(
        ProductList MeetupMenu, ProductList WorkshopMenu,
        ProductList StandardMenu, ProductList MorningMenu);

    private async Task<ProductListRefs> SeedProductListsAsync(ProductRefs products)
    {
        var meetupMenu = ProductList.Create("Menu Shiba Meet-up");
        var workshopMenu = ProductList.Create("Menu Paw Painting");
        var standardMenu = ProductList.Create("Menu standardowe");
        var morningMenu = ProductList.Create("Menu poranne");
        db.ProductLists.AddRange(meetupMenu, workshopMenu, standardMenu, morningMenu);
        await db.SaveChangesAsync();

        db.ProductListItems.AddRange(
            ProductListItem.Create(meetupMenu.Id, products.MatchaLatte.Id),
            ProductListItem.Create(meetupMenu.Id, products.HojichaLatte.Id),
            ProductListItem.Create(meetupMenu.Id, products.Cappuccino.Id),
            ProductListItem.Create(meetupMenu.Id, products.ShibaCookie.Id),

            ProductListItem.Create(workshopMenu.Id, products.YuzuLemoniada.Id),
            ProductListItem.Create(workshopMenu.Id, products.MochiDonut.Id),
            ProductListItem.Create(workshopMenu.Id, products.Cheesecake.Id),
            ProductListItem.Create(workshopMenu.Id, products.OnigiriSalmon.Id),

            ProductListItem.Create(standardMenu.Id, products.MatchaLatte.Id),
            ProductListItem.Create(standardMenu.Id, products.HojichaLatte.Id),
            ProductListItem.Create(standardMenu.Id, products.Cappuccino.Id),
            ProductListItem.Create(standardMenu.Id, products.YuzuLemoniada.Id),
            ProductListItem.Create(standardMenu.Id, products.ShibaCookie.Id),
            ProductListItem.Create(standardMenu.Id, products.MochiDonut.Id),
            ProductListItem.Create(standardMenu.Id, products.Cheesecake.Id),
            ProductListItem.Create(standardMenu.Id, products.OnigiriSalmon.Id),

            ProductListItem.Create(morningMenu.Id, products.Cappuccino.Id),
            ProductListItem.Create(morningMenu.Id, products.MatchaLatte.Id),
            ProductListItem.Create(morningMenu.Id, products.ShibaCookie.Id),
            ProductListItem.Create(morningMenu.Id, products.Cheesecake.Id));

        await db.SaveChangesAsync();
        return new ProductListRefs(meetupMenu, workshopMenu, standardMenu, morningMenu);
    }

    public sealed record StaffRefs(Guid ManagerUserId, Guid BaristaUserId);

    private async Task<StaffRefs> SeedStaffAsync(string staffPassword)
    {
        var managerRole = await CreateRoleWithPermissionsAsync("Kierownik",
        [
            Permissions.OutletManage, Permissions.TablesManage,
            Permissions.ProductsManage, Permissions.ModifiersManage,
            Permissions.MenusManage, Permissions.TaxRatesManage,
            Permissions.PricingManage, Permissions.SalesChannelsManage,
            Permissions.PromotionsManage, Permissions.EventsManage,
            Permissions.UsersManage, Permissions.PrintoutTemplatesManage,
            Permissions.OrdersView, Permissions.OrdersManage,
            Permissions.ReportsView, Permissions.LoyaltyManage
        ]);
        var baristaRole = await CreateRoleWithPermissionsAsync("Barista",
        [
            Permissions.OrdersView, Permissions.OrdersManage,
            Permissions.LoyaltyManage, Permissions.ProductsManage
        ]);

        var manager = await CreateStaffAsync(ManagerEmail, "Maja", "Kowalska", staffPassword, managerRole.Id);
        var barista = await CreateStaffAsync(BaristaEmail, "Anna", "Nowak", staffPassword, baristaRole.Id);

        return new StaffRefs(manager.Id, barista.Id);
    }

    private async Task<AppRole> CreateRoleWithPermissionsAsync(string roleName, string[] permissions)
    {
        var role = AppRole.Create(roleName);
        var roleResult = await roleManager.CreateAsync(role);
        if (!roleResult.Succeeded)
            throw new DomainException(string.Join("; ", roleResult.Errors.Select(x => x.Description)));

        foreach (var permission in permissions)
            db.RoleClaims.Add(new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = ClaimsPrincipalExtensions.PermissionClaim,
                ClaimValue = permission
            });

        await db.SaveChangesAsync();
        return role;
    }

    private async Task<AppUser> CreateStaffAsync(string email, string firstName, string lastName, string password, Guid roleId)
    {
        var user = AppUser.Create(email, firstName, lastName, AccountType.Staff);
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new DomainException(string.Join("; ", createResult.Errors.Select(x => x.Description)));

        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = roleId });
        await db.SaveChangesAsync();
        return user;
    }

    public sealed record CustomerRefs(AppUser Zofia, AppUser Jakub);

    private async Task<CustomerRefs> SeedCustomersAsync(string password)
    {
        var zofia = await CreateGuestAsync(Customer1Email, "Zofia", "Wiśniewska", password);
        var jakub = await CreateGuestAsync(Customer2Email, "Jakub", "Lewandowski", password);
        return new CustomerRefs(zofia, jakub);
    }

    private async Task<AppUser> CreateGuestAsync(string email, string firstName, string lastName, string password)
    {
        var user = AppUser.Create(email, firstName, lastName, AccountType.Guest);
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new DomainException(string.Join("; ", result.Errors.Select(x => x.Description)));
        return user;
    }

    private async Task SeedLoyaltyAsync(CustomerRefs customers)
    {
        db.LoyaltyPointLogs.AddRange(
            LoyaltyPointLog.Create(customers.Zofia.Id, 50, "Zakup w lokalu"),
            LoyaltyPointLog.Create(customers.Zofia.Id, 100, "Udział w wydarzeniu"),
            LoyaltyPointLog.Create(customers.Zofia.Id, -80, "Wykorzystanie punktów"));

        db.LoyaltyPointLogs.Add(
            LoyaltyPointLog.Create(customers.Jakub.Id, 30, "Pierwsze zamówienie"));

        await db.SaveChangesAsync();
    }

    public sealed record EventRefs(Event ShibaMeetup, Event PawPainting, Event CoffeeDayToday);

    private async Task<EventRefs> SeedEventsAsync(ProductListRefs productLists, PriceGroupRefs priceGroups)
    {
        var now = DateTimeOffset.UtcNow;

        var shibaMeetup = Event.Create(
            "Shiba Meet-up",
            description: "Niedziela pełna Shib! 🐕 Kawa parzona z sercem, herbata rzemieślnicza i całe stado puszystych ogonków w jednym miejscu. Przyprowadź swojego pupila albo po prostu wpadnij poprzytulać cudze — gwarantujemy uśmiech od ucha do ucha.",
            imageUrl: null,
            productListId: productLists.MeetupMenu.Id,
            priceGroupId: priceGroups.Standard.Id);
        db.Events.Add(shibaMeetup);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(shibaMeetup.Id, DateOnly.FromDateTime(now.AddDays(-10).Date)));
        shibaMeetup.Publish(now.AddDays(-17));
        shibaMeetup.Close(now.AddDays(-9));

        var pawPainting = Event.Create(
            "Paw Painting Workshop",
            description: "Zamień łapkę pupila w dzieło sztuki! 🐾🎨 Na naszych warsztatach stworzysz pamiątkowy odcisk łapy w otoczeniu kawy i dobrej energii. Wychodzisz z unikatową pamiątką, którą pokochasz na lata.",
            imageUrl: null,
            productListId: productLists.WorkshopMenu.Id,
            priceGroupId: priceGroups.Standard.Id);
        db.Events.Add(pawPainting);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(pawPainting.Id, new DateOnly(2026, 6, 14)));
        pawPainting.Publish(now.AddDays(-25));
        pawPainting.Close(now.AddDays(-22));

        var coffeeDay = Event.Create(
            "Matcha Matsuri",
            description: "Cały dzień w zieleni matchi! 🍵 Ceremonialna matcha ubijana na miejscu, matcha latte i matchowe słodkości. Znajdź swój ulubiony odcień zieleni — dziś w specjalnych cenach wydarzenia.",
            imageUrl: null,
            productListId: productLists.MeetupMenu.Id,
            priceGroupId: priceGroups.Promo.Id);
        db.Events.Add(coffeeDay);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(coffeeDay.Id, DateOnly.FromDateTime(now.Date)));
        coffeeDay.Publish(now.AddDays(-1));

        var puppyYoga = Event.Create(
            "Hanami — pod kwitnącą wiśnią",
            description: "Świętujemy sezon sakury! 🌸 Różowa dekoracja, limitowane napoje o smaku kwiatu wiśni i japońskie przekąski. Usiądź, zwolnij i poczuj klimat wiosny w Tokio — bez lotu samolotem.",
            imageUrl: null,
            productListId: productLists.WorkshopMenu.Id,
            priceGroupId: priceGroups.Standard.Id);
        db.Events.Add(puppyYoga);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(puppyYoga.Id, DateOnly.FromDateTime(now.AddDays(5).Date)));
        puppyYoga.Publish(now.AddDays(-1));

        var latteArt = Event.Create(
            "Barista Latte Art Show",
            description: "Bariści stają w szranki! ☕🎨 Pokaz latte art na żywo, w którym mleko zamienia się w małe dzieła sztuki. Degustujesz, kibicujesz i to Twój głos decyduje, który wzór zgarnie tytuł mistrza dnia.",
            imageUrl: null,
            productListId: productLists.MeetupMenu.Id,
            priceGroupId: priceGroups.Standard.Id);
        db.Events.Add(latteArt);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(latteArt.Id, DateOnly.FromDateTime(now.AddDays(12).Date)));
        latteArt.Publish(now.AddDays(-1));

        await db.SaveChangesAsync();
        return new EventRefs(shibaMeetup, pawPainting, coffeeDay);
    }

    public sealed record PromotionRefs(PromotionCode Welcome10, PromotionCode Shiba15);

    private async Task<PromotionRefs> SeedPromotionCodesAsync()
    {
        var welcome10 = PromotionCode.Create("WITAJ10", 10m, maxUses: 1);
        var shiba15 = PromotionCode.Create("SHIBA15", 15m,
            validUntil: new DateTimeOffset(2026, 6, 30, 23, 59, 59, TimeSpan.Zero));
        db.PromotionCodes.AddRange(welcome10, shiba15);
        await db.SaveChangesAsync();
        return new PromotionRefs(welcome10, shiba15);
    }

    private async Task SeedSampleOrdersAsync(
        Guid outletId,
        ProductRefs products,
        CustomerRefs customers,
        TableRefs tables,
        SalesChannelRefs salesChannels,
        PromotionRefs promotions,
        EventRefs events)
    {
        var now = DateTimeOffset.UtcNow;

        var dineIn = Order.Create(outletId, tableId: tables.Bar.Id, salesChannelId: salesChannels.DineIn.Id, userId: customers.Zofia.Id);
        db.Orders.Add(dineIn);
        await db.SaveChangesAsync();
        AddOrderLine(dineIn.Id, products.Cappuccino, 2, 14.00m, 0.08m);
        AddOrderLine(dineIn.Id, products.Cheesecake, 1, 22.00m, 0.08m);
        dineIn.Close(now);

        var takeaway = Order.Create(outletId, salesChannelId: salesChannels.Takeout.Id, userId: customers.Jakub.Id);
        db.Orders.Add(takeaway);
        await db.SaveChangesAsync();
        AddOrderLine(takeaway.Id, products.YuzuLemoniada, 1, 16.00m, 0.05m);
        AddOrderLine(takeaway.Id, products.OnigiriSalmon, 2, 12.00m, 0.05m);
        takeaway.Close(now.AddHours(-1));

        var pickup = Order.Create(outletId, salesChannelId: salesChannels.Takeout.Id, userId: customers.Zofia.Id);
        db.Orders.Add(pickup);
        await db.SaveChangesAsync();
        AddOrderLine(pickup.Id, products.MatchaLatte, 1, 18.00m, 0.05m);
        AddOrderLine(pickup.Id, products.MochiDonut, 1, 14.00m, 0.05m);

        var cancelled = Order.Create(outletId, tableId: tables.Window.Id, salesChannelId: salesChannels.DineIn.Id);
        db.Orders.Add(cancelled);
        await db.SaveChangesAsync();
        AddOrderLine(cancelled.Id, products.Cappuccino, 1, 14.00m, 0.08m);
        cancelled.Cancel(now.AddHours(-3), "Klient nie wrócił po napój.");

        var promoOrder = Order.Create(outletId, tableId: tables.Tatami.Id, salesChannelId: salesChannels.DineIn.Id,
            userId: customers.Zofia.Id);
        db.Orders.Add(promoOrder);
        await db.SaveChangesAsync();
        AddOrderLine(promoOrder.Id, products.HojichaLatte, 1, 18.00m, 0.08m);
        AddOrderLine(promoOrder.Id, products.Cheesecake, 1, 22.00m, 0.08m);
        promoOrder.RedeemLoyaltyPoints(20, subtotal: 40.00m);
        promoOrder.AssignPromotion(promotions.Welcome10.Id, discount: 4.32m);
        promotions.Welcome10.RegisterUsage();
        promoOrder.Close(now.AddMinutes(-30));

        await db.SaveChangesAsync();

        var eatIn = salesChannels.DineIn.Id;
        var takeout = salesChannels.Takeout.Id;

        await SeedClosedOrderAsync(outletId, eatIn, tables.Bar.Id, customers.Zofia.Id, events.ShibaMeetup.Id,
            now.AddDays(-10).AddHours(2),
            (products.MatchaLatte, 2, 18.00m, 0.08m), (products.ShibaCookie, 2, 12.00m, 0.08m));
        await SeedClosedOrderAsync(outletId, eatIn, tables.Window.Id, customers.Jakub.Id, events.ShibaMeetup.Id,
            now.AddDays(-10).AddHours(3),
            (products.Cappuccino, 3, 14.00m, 0.08m), (products.HojichaLatte, 1, 18.00m, 0.08m));
        await SeedClosedOrderAsync(outletId, eatIn, tables.Tatami.Id, null, events.ShibaMeetup.Id,
            now.AddDays(-10).AddHours(4),
            (products.HojichaLatte, 2, 18.00m, 0.08m), (products.ShibaCookie, 1, 12.00m, 0.08m));

        var pawDay = new DateTimeOffset(2026, 6, 14, 12, 0, 0, TimeSpan.Zero);
        await SeedClosedOrderAsync(outletId, eatIn, tables.Bar.Id, customers.Zofia.Id, events.PawPainting.Id,
            pawDay.AddHours(1),
            (products.YuzuLemoniada, 2, 16.00m, 0.08m), (products.Cheesecake, 2, 22.00m, 0.08m));
        await SeedClosedOrderAsync(outletId, takeout, null, customers.Jakub.Id, events.PawPainting.Id,
            pawDay.AddHours(2),
            (products.MochiDonut, 3, 14.00m, 0.05m), (products.OnigiriSalmon, 2, 12.00m, 0.05m));

        await SeedClosedOrderAsync(outletId, eatIn, tables.Bar.Id, customers.Zofia.Id, events.CoffeeDayToday.Id,
            now.AddHours(-4),
            (products.MatchaLatte, 1, 18.00m, 0.08m), (products.Cappuccino, 2, 14.00m, 0.08m));
        await SeedClosedOrderAsync(outletId, takeout, null, customers.Jakub.Id, events.CoffeeDayToday.Id,
            now.AddHours(-2),
            (products.HojichaLatte, 2, 18.00m, 0.05m), (products.ShibaCookie, 2, 12.00m, 0.05m));

        await SeedClosedOrderAsync(outletId, eatIn, tables.Window.Id, customers.Jakub.Id, null,
            now.AddDays(-5).AddHours(1),
            (products.Cappuccino, 1, 14.00m, 0.08m), (products.MochiDonut, 2, 14.00m, 0.08m));
        await SeedClosedOrderAsync(outletId, takeout, null, customers.Zofia.Id, null,
            now.AddDays(-3).AddHours(2),
            (products.YuzuLemoniada, 2, 16.00m, 0.05m), (products.OnigiriSalmon, 1, 12.00m, 0.05m));
        await SeedClosedOrderAsync(outletId, eatIn, tables.Tatami.Id, null, null,
            now.AddDays(-1).AddHours(5),
            (products.Cheesecake, 1, 22.00m, 0.08m), (products.MatchaLatte, 2, 18.00m, 0.08m));

        await db.SaveChangesAsync();
    }

    private async Task<Order> SeedClosedOrderAsync(
        Guid outletId,
        Guid salesChannelId,
        Guid? tableId,
        Guid? userId,
        Guid? eventId,
        DateTimeOffset closedAt,
        params (Product Product, int Quantity, decimal Gross, decimal VatRate)[] lines)
    {
        var order = Order.Create(outletId, tableId: tableId, salesChannelId: salesChannelId, userId: userId, eventId: eventId);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        foreach (var line in lines)
            AddOrderLine(order.Id, line.Product, line.Quantity, line.Gross, line.VatRate);

        order.Close(closedAt);
        return order;
    }

    private void AddOrderLine(Guid orderId, Product product, int quantity, decimal grossPerOne, decimal vatRate)
    {
        var netPerOne = Math.Round(grossPerOne / (1m + vatRate), 2);
        var vatPerOne = grossPerOne - netPerOne;
        db.OrderLines.Add(OrderLine.Create(orderId, product.Id, quantity, netPerOne, vatPerOne));
    }

    private async Task SeedPrintoutTemplatesAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "Templates", "potwierdzenie-wydarzenia.docx");
        var bytes = await File.ReadAllBytesAsync(path);
        db.PrintoutTemplates.Add(
            PrintoutTemplate.Create(
                "Potwierdzenie wydarzenia",
                "potwierdzenie-wydarzenia.docx",
                PrintoutFile.DocxContentType,
                bytes));
        await db.SaveChangesAsync();
    }

    private async Task BackdateAuditsForVarietyAsync(Guid[] creatorUserIds)
    {
        string[] tables =
        [
            "tags", "allergens", "tax_rates",
            "modifier_groups", "modifiers",
            "price_groups", "sales_channels",
            "tables", "products",
            "promotion_codes", "events",
            "product_lists", "printout_templates"
        ];

        foreach (var table in tables)
        {
            var sql = $$"""
                WITH numbered AS (
                    SELECT id, ROW_NUMBER() OVER (ORDER BY created_at, id) AS rn
                    FROM {{table}}
                )
                UPDATE {{table}} AS t SET
                    created_at = NOW() - (numbered.rn * INTERVAL '36 hours'),
                    created_by = (ARRAY[{0}::uuid, {1}::uuid, {2}::uuid])[((numbered.rn - 1) % 3 + 1)::int]
                FROM numbered
                WHERE t.id = numbered.id;
                """;

            await db.Database.ExecuteSqlRawAsync(
                sql,
                creatorUserIds[0], creatorUserIds[1], creatorUserIds[2]);
        }
    }
}
