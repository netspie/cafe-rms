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

// Whole-cafe demo seed for Yumeya, the single themed Japanese café.
// Idempotent: bails out if the outlet is already there.
public class YumeyaDemoSeeder(
    AppDbContext db,
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IConfiguration config,
    ILogger<YumeyaDemoSeeder> logger)
{
    private const string OwnerEmail = "dariusz@yumeya.pl";
    private const string OutletDisplayName = "Yumeya Marszałkowska";
    private const string OutletAddress = "ul. Marszałkowska 100, 00-001 Warszawa";

    public async Task SeedAsync()
    {
        var ownerPassword = config["Seed:YumeyaOwnerPassword"];
        if (string.IsNullOrWhiteSpace(ownerPassword))
        {
            logger.LogWarning("Seed:YumeyaOwnerPassword not set; skipping Yumeya demo seed.");
            return;
        }

        var isAlreadySeeded = await db.Outlets.AnyAsync(x => x.DisplayName == OutletDisplayName);
        if (isAlreadySeeded)
            return;

        var (ownerUserId, outletId) = await ProvisionAsync(ownerPassword);

        var taxRates = await SeedTaxRatesAsync();
        var tags = await SeedTagsAsync();
        var allergens = await SeedAllergensAsync();
        var modifierGroups = await SeedModifierGroupsAsync();
        var priceGroups = await SeedPriceGroupsAsync();
        var salesChannels = await SeedSalesChannelsAsync(priceGroups);
        var tables = await SeedTablesAsync(outletId);
        var products = await SeedProductsAsync(taxRates, tags, allergens, modifierGroups, priceGroups);
        var staff = await SeedStaffAsync(ownerPassword);
        var customers = await SeedCustomersAsync(ownerPassword);
        await SeedLoyaltyAsync(customers);
        await SeedEventsAsync();
        var promotions = await SeedPromotionCodesAsync();
        await SeedSampleOrdersAsync(outletId, products, customers, tables, salesChannels, promotions);
        await SeedPrintoutTemplatesAsync();

        await db.SaveChangesAsync();

        await BackdateAuditsForVarietyAsync(
            [ownerUserId, staff.ManagerUserId, staff.Barista1UserId, staff.Barista2UserId]);

        logger.LogInformation(
            "Seeded Yumeya demo (OutletId={OutletId}, OwnerUserId={OwnerUserId})",
            outletId, ownerUserId);
    }

    private async Task<(Guid OwnerUserId, Guid OutletId)> ProvisionAsync(string ownerPassword)
    {
        // Owner role must exist before we assign the owner user to it.
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
            Currency.PLN);
        db.Outlets.Add(outlet);
        await db.SaveChangesAsync();

        return (owner.Id, outlet.Id);
    }

    // === Reference data ===

    public sealed record TaxRateRefs(TaxRate EatIn, TaxRate Takeaway, TaxRate Goods);

    private async Task<TaxRateRefs> SeedTaxRatesAsync()
    {
        var eatIn = TaxRate.Create("VAT 8%", "Gastronomia na miejscu", 8m);
        var takeaway = TaxRate.Create("VAT 5%", "Żywność na wynos", 5m);
        var goods = TaxRate.Create("VAT 23%", "Towary (kubki, torby)", 23m);
        db.TaxRates.AddRange(eatIn, takeaway, goods);
        await db.SaveChangesAsync();
        return new TaxRateRefs(eatIn, takeaway, goods);
    }

    public sealed record TagRefs(
        Tag HotDrinks, Tag IcedDrinks, Tag Coffee, Tag MatchaTea,
        Tag Cakes, Tag Wagashi, Tag Savoury,
        Tag Seasonal, Tag Vegan, Tag GlutenFree);

    private async Task<TagRefs> SeedTagsAsync()
    {
        var hotDrinks = Tag.Create("Napoje gorące");
        var icedDrinks = Tag.Create("Napoje zimne");
        var coffee = Tag.Create("Kawa");
        var matchaTea = Tag.Create("Matcha & herbata");
        var cakes = Tag.Create("Ciasta");
        var wagashi = Tag.Create("Wagashi");
        var savoury = Tag.Create("Przekąski");
        var seasonal = Tag.Create("Sezonowe");
        var vegan = Tag.Create("Wegańskie");
        var glutenFree = Tag.Create("Bezglutenowe");
        db.Tags.AddRange(hotDrinks, icedDrinks, coffee, matchaTea, cakes, wagashi, savoury, seasonal, vegan, glutenFree);
        await db.SaveChangesAsync();
        return new TagRefs(hotDrinks, icedDrinks, coffee, matchaTea, cakes, wagashi, savoury, seasonal, vegan, glutenFree);
    }

    public sealed record AllergenRefs(
        Allergen Milk, Allergen Egg, Allergen Gluten, Allergen Soy,
        Allergen Sesame, Allergen TreeNuts, Allergen Peanuts);

    private async Task<AllergenRefs> SeedAllergensAsync()
    {
        var milk = Allergen.Create("Mleko");
        var egg = Allergen.Create("Jajka");
        var gluten = Allergen.Create("Gluten");
        var soy = Allergen.Create("Soja");
        var sesame = Allergen.Create("Sezam");
        var treeNuts = Allergen.Create("Orzechy");
        var peanuts = Allergen.Create("Orzeszki ziemne");
        db.Allergens.AddRange(milk, egg, gluten, soy, sesame, treeNuts, peanuts);
        await db.SaveChangesAsync();
        return new AllergenRefs(milk, egg, gluten, soy, sesame, treeNuts, peanuts);
    }

    public sealed record ModifierGroupRefs(
        ModifierGroup Size, ModifierGroup Milk, ModifierGroup Sweetness,
        ModifierGroup Temperature, ModifierGroup ExtraShot,
        ModifierGroup WhippedCream, ModifierGroup ExtraSweet);

    private async Task<ModifierGroupRefs> SeedModifierGroupsAsync()
    {
        var size = ModifierGroup.Create("Rozmiar");
        var milk = ModifierGroup.Create("Mleko");
        var sweetness = ModifierGroup.Create("Słodkość");
        var temperature = ModifierGroup.Create("Temperatura");
        var extraShot = ModifierGroup.Create("Dodatkowy shot");
        var whippedCream = ModifierGroup.Create("Bita śmietana");
        var extraSweet = ModifierGroup.Create("Dodatki deserowe");

        db.ModifierGroups.AddRange(size, milk, sweetness, temperature, extraShot, whippedCream, extraSweet);
        await db.SaveChangesAsync();

        db.Modifiers.AddRange(
            Modifier.Create("Standard", size.Id),
            Modifier.Create("Duży", size.Id),

            Modifier.Create("Krowie", milk.Id),
            Modifier.Create("Owsiane", milk.Id),
            Modifier.Create("Sojowe", milk.Id),
            Modifier.Create("Migdałowe", milk.Id),

            Modifier.Create("Brak", sweetness.Id),
            Modifier.Create("Mała", sweetness.Id),
            Modifier.Create("Standardowa", sweetness.Id),
            Modifier.Create("Większa", sweetness.Id),

            Modifier.Create("Gorące", temperature.Id),
            Modifier.Create("Mrożone", temperature.Id),

            Modifier.Create("Bez", extraShot.Id),
            Modifier.Create("Dodatkowy shot", extraShot.Id),

            Modifier.Create("Bez", whippedCream.Id),
            Modifier.Create("Z bitą śmietaną", whippedCream.Id),

            Modifier.Create("Bez dodatków", extraSweet.Id),
            Modifier.Create("Dodatkowe anko", extraSweet.Id),
            Modifier.Create("Dodatkowe mochi", extraSweet.Id));

        await db.SaveChangesAsync();
        return new ModifierGroupRefs(size, milk, sweetness, temperature, extraShot, whippedCream, extraSweet);
    }

    // === Pricing ===

    public sealed record PriceGroupRefs(PriceGroup Standard, PriceGroup Loyalty, PriceGroup HappyHour);

    private async Task<PriceGroupRefs> SeedPriceGroupsAsync()
    {
        var standard = PriceGroup.Create("Standardowa");
        var loyalty = PriceGroup.Create("Lojalność");
        var happyHour = PriceGroup.Create("Happy hour");
        db.PriceGroups.AddRange(standard, loyalty, happyHour);
        await db.SaveChangesAsync();
        return new PriceGroupRefs(standard, loyalty, happyHour);
    }

    public sealed record SalesChannelRefs(SalesChannel Pos, SalesChannel Online, SalesChannel Mobile);

    private async Task<SalesChannelRefs> SeedSalesChannelsAsync(PriceGroupRefs priceGroups)
    {
        var pos = SalesChannel.Create("Lokal (POS)", isTakeout: false);
        var online = SalesChannel.Create("Online", isTakeout: true);
        var mobile = SalesChannel.Create("Aplikacja mobilna", isTakeout: true);
        db.SalesChannels.AddRange(pos, online, mobile);
        await db.SaveChangesAsync();

        db.SalesChannelPriceGroups.AddRange(
            SalesChannelPriceGroup.Create(pos.Id, priceGroups.Standard.Id),
            SalesChannelPriceGroup.Create(pos.Id, priceGroups.HappyHour.Id),
            SalesChannelPriceGroup.Create(online.Id, priceGroups.Standard.Id),
            SalesChannelPriceGroup.Create(mobile.Id, priceGroups.Standard.Id),
            SalesChannelPriceGroup.Create(mobile.Id, priceGroups.Loyalty.Id));

        await db.SaveChangesAsync();
        return new SalesChannelRefs(pos, online, mobile);
    }

    // === Outlet floor plan ===

    public sealed record TableRefs(
        Table Bar1, Table Bar2, Table Bar3,
        Table Window1, Table Window2,
        Table Tatami,
        Table Patio1, Table Patio2);

    private async Task<TableRefs> SeedTablesAsync(Guid outletId)
    {
        var bar1 = Table.Create("Bar 1", outletId);
        var bar2 = Table.Create("Bar 2", outletId);
        var bar3 = Table.Create("Bar 3", outletId);
        var window1 = Table.Create("Okno 1", outletId);
        var window2 = Table.Create("Okno 2", outletId);
        var tatami = Table.Create("Tatami", outletId);
        var patio1 = Table.Create("Ogródek 1", outletId);
        var patio2 = Table.Create("Ogródek 2", outletId);
        db.Tables.AddRange(bar1, bar2, bar3, window1, window2, tatami, patio1, patio2);
        await db.SaveChangesAsync();
        return new TableRefs(bar1, bar2, bar3, window1, window2, tatami, patio1, patio2);
    }

    // === Products ===

    public sealed record ProductRefs(
        Product MatchaLatte, Product HojichaLatte, Product KuroGomaLatte, Product SakuraLatte,
        Product YuzuLemoniada, Product UmeSoda, Product IchigoMilk,
        Product KawaParzona, Product Espresso, Product Cappuccino, Product EarlGrey, Product Genmaicha,
        Product IchigoShortcake, Product MatchaTiramisu, Product YuzuBasque, Product MontBlanc,
        Product HojichaRoll, Product KuroGomaOpera,
        Product MochiDonut, Product IchigoDaifuku, Product Dorayaki, Product SakuraMochi,
        Product TamagoSando, Product KatsuSando, Product OnigiriSalmon, Product OnigiriUmeboshi, Product ZupaMiso);

    private async Task<ProductRefs> SeedProductsAsync(
        TaxRateRefs taxRates,
        TagRefs tags,
        AllergenRefs allergens,
        ModifierGroupRefs modifierGroups,
        PriceGroupRefs priceGroups)
    {
        var matchaLatte = AddProduct("Matcha latte", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea, tags.Vegan],
            [allergens.Soy],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature, modifierGroups.ExtraShot, modifierGroups.WhippedCream]);
        var hojichaLatte = AddProduct("Hojicha latte", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var kuroGomaLatte = AddProduct("Kuro goma latte", taxRates.EatIn.Id, 19.00m, priceGroups,
            [tags.HotDrinks],
            [allergens.Milk, allergens.Sesame],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var sakuraLatte = AddProduct("Sakura latte", taxRates.EatIn.Id, 20.00m, priceGroups,
            [tags.HotDrinks, tags.Seasonal],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var yuzuLemoniada = AddProduct("Yuzu lemoniada", taxRates.EatIn.Id, 16.00m, priceGroups,
            [tags.IcedDrinks, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness]);
        var umeSoda = AddProduct("Ume soda", taxRates.EatIn.Id, 16.00m, priceGroups,
            [tags.IcedDrinks, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness]);
        var ichigoMilk = AddProduct("Ichigo milk", taxRates.EatIn.Id, 17.00m, priceGroups,
            [tags.IcedDrinks],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var kawaParzona = AddProduct("Kawa parzona", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.HotDrinks, tags.Coffee, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var espresso = AddProduct("Espresso", taxRates.EatIn.Id, 9.00m, priceGroups,
            [tags.HotDrinks, tags.Coffee, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.ExtraShot]);
        var cappuccino = AddProduct("Cappuccino", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.HotDrinks, tags.Coffee],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var earlGrey = AddProduct("Earl grey", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var genmaicha = AddProduct("Genmaicha", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness, modifierGroups.Temperature]);

        var ichigoShortcake = AddProduct("Ichigo shortcake", taxRates.EatIn.Id, 22.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            [modifierGroups.WhippedCream, modifierGroups.ExtraSweet]);
        var matchaTiramisu = AddProduct("Matcha tiramisu", taxRates.EatIn.Id, 24.00m, priceGroups,
            [tags.Cakes, tags.MatchaTea],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            [modifierGroups.ExtraSweet]);
        var yuzuBasque = AddProduct("Yuzu basque cheesecake", taxRates.EatIn.Id, 26.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg],
            [modifierGroups.ExtraSweet]);
        var montBlanc = AddProduct("Mont Blanc", taxRates.EatIn.Id, 28.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten, allergens.TreeNuts],
            [modifierGroups.WhippedCream]);
        var hojichaRoll = AddProduct("Hojicha roll cake", taxRates.EatIn.Id, 22.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            [modifierGroups.WhippedCream]);
        var kuroGomaOpera = AddProduct("Kuro goma opera", taxRates.EatIn.Id, 26.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten, allergens.Sesame],
            [modifierGroups.ExtraSweet]);
        var mochiDonut = AddProduct("Mochi donut", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.Cakes, tags.Wagashi],
            [allergens.Gluten, allergens.Milk],
            [modifierGroups.Sweetness]);
        var ichigoDaifuku = AddProduct("Ichigo daifuku", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.Wagashi, tags.GlutenFree],
            [],
            [modifierGroups.ExtraSweet]);
        var dorayaki = AddProduct("Dorayaki", taxRates.EatIn.Id, 10.00m, priceGroups,
            [tags.Wagashi],
            [allergens.Egg, allergens.Gluten],
            [modifierGroups.ExtraSweet]);
        var sakuraMochi = AddProduct("Sakura mochi", taxRates.EatIn.Id, 11.00m, priceGroups,
            [tags.Wagashi, tags.Seasonal, tags.GlutenFree],
            [],
            [modifierGroups.ExtraSweet]);

        var tamagoSando = AddProduct("Tamago sando", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.Savoury],
            [allergens.Egg, allergens.Gluten, allergens.Milk],
            []);
        var katsuSando = AddProduct("Mini katsu sando", taxRates.EatIn.Id, 22.00m, priceGroups,
            [tags.Savoury],
            [allergens.Gluten, allergens.Milk, allergens.Soy],
            []);
        var onigiriSalmon = AddProduct("Onigiri łosoś", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.Savoury, tags.GlutenFree],
            [],
            []);
        var onigiriUmeboshi = AddProduct("Onigiri umeboshi", taxRates.EatIn.Id, 10.00m, priceGroups,
            [tags.Savoury, tags.Vegan, tags.GlutenFree],
            [],
            []);
        var zupaMiso = AddProduct("Zupa miso", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.Savoury, tags.Vegan, tags.GlutenFree],
            [allergens.Soy],
            [modifierGroups.Temperature]);

        AddProduct("Kubek emaliowany Yumeya", taxRates.Goods.Id, 65.00m, priceGroups, [], [], []);
        AddProduct("Furoshiki", taxRates.Goods.Id, 95.00m, priceGroups, [], [], []);
        AddProduct("Puszka matcha 50g", taxRates.Goods.Id, 110.00m, priceGroups, [], [], []);

        await db.SaveChangesAsync();

        var mainMenu = ProductList.Create("Menu główne");
        db.ProductLists.Add(mainMenu);
        await db.SaveChangesAsync();

        Product[] menuOrder =
        [
            matchaLatte, hojichaLatte, kuroGomaLatte, sakuraLatte,
            yuzuLemoniada, umeSoda, ichigoMilk,
            kawaParzona, espresso, cappuccino, earlGrey, genmaicha,
            ichigoShortcake, matchaTiramisu, yuzuBasque, montBlanc, hojichaRoll, kuroGomaOpera,
            mochiDonut, ichigoDaifuku, dorayaki, sakuraMochi,
            tamagoSando, katsuSando, onigiriSalmon, onigiriUmeboshi, zupaMiso
        ];

        foreach (var product in menuOrder)
            db.ProductListItems.Add(ProductListItem.Create(mainMenu.Id, product.Id));

        await db.SaveChangesAsync();

        return new ProductRefs(
            matchaLatte, hojichaLatte, kuroGomaLatte, sakuraLatte,
            yuzuLemoniada, umeSoda, ichigoMilk,
            kawaParzona, espresso, cappuccino, earlGrey, genmaicha,
            ichigoShortcake, matchaTiramisu, yuzuBasque, montBlanc, hojichaRoll, kuroGomaOpera,
            mochiDonut, ichigoDaifuku, dorayaki, sakuraMochi,
            tamagoSando, katsuSando, onigiriSalmon, onigiriUmeboshi, zupaMiso);
    }

    private Product AddProduct(
        string name,
        Guid taxRateId,
        decimal standardNet,
        PriceGroupRefs priceGroups,
        Tag[] productTags,
        Allergen[] productAllergens,
        ModifierGroup[] productModifierGroups)
    {
        var product = Product.Create(name, taxRateId);
        db.Products.Add(product);

        foreach (var tag in productTags)
            db.ProductTags.Add(ProductTag.Create(product.Id, tag.Id));

        foreach (var allergen in productAllergens)
            db.ProductAllergens.Add(ProductAllergen.Create(product.Id, allergen.Id));

        foreach (var modifierGroup in productModifierGroups)
            db.ProductModifierGroups.Add(ProductModifierGroup.Create(product.Id, modifierGroup.Id));

        db.ProductPrices.AddRange(
            ProductPrice.Create(product.Id, priceGroups.Standard.Id, standardNet),
            ProductPrice.Create(product.Id, priceGroups.Loyalty.Id, Discount(standardNet, 0.10m)),
            ProductPrice.Create(product.Id, priceGroups.HappyHour.Id, Discount(standardNet, 0.15m)));

        return product;
    }

    private static decimal Discount(decimal net, decimal percentage) =>
        Math.Round(net * (1m - percentage), 2);

    // === Staff ===

    public sealed record StaffRefs(Guid ManagerUserId, Guid Barista1UserId, Guid Barista2UserId, Guid KitchenUserId);

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
        var kitchenRole = await CreateRoleWithPermissionsAsync("Kuchnia",
        [
            Permissions.OrdersView
        ]);

        var manager = await CreateStaffAsync("kierownik@yumeya.pl", "Maja", "Kowalska", staffPassword, managerRole.Id);
        var barista1 = await CreateStaffAsync("anna@yumeya.pl", "Anna", "Nowak", staffPassword, baristaRole.Id);
        var barista2 = await CreateStaffAsync("kenji@yumeya.pl", "Kenji", "Tanaka", staffPassword, baristaRole.Id);
        var kitchen = await CreateStaffAsync("yuna@yumeya.pl", "Yuna", "Kim", staffPassword, kitchenRole.Id);

        return new StaffRefs(manager.Id, barista1.Id, barista2.Id, kitchen.Id);
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

    // === Customers ===

    public sealed record CustomerRefs(AppUser Sakura, AppUser Yuki, AppUser Hana);

    private async Task<CustomerRefs> SeedCustomersAsync(string password)
    {
        var sakura = await CreateGuestAsync("sakura@example.jp", "Sakura", "Yamamoto", password);
        var yuki = await CreateGuestAsync("yuki@example.jp", "Yuki", "Watanabe", password);
        var hana = await CreateGuestAsync("hana@example.jp", "Hana", "Sato", password);
        return new CustomerRefs(sakura, yuki, hana);
    }

    private async Task<AppUser> CreateGuestAsync(string email, string firstName, string lastName, string password)
    {
        var user = AppUser.Create(email, firstName, lastName, AccountType.Guest);
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new DomainException(string.Join("; ", result.Errors.Select(x => x.Description)));
        return user;
    }

    // === Loyalty ===

    private async Task SeedLoyaltyAsync(CustomerRefs customers)
    {
        db.LoyaltyPointLogs.AddRange(
            LoyaltyPointLog.Create(customers.Sakura.Id, 50, "Zakup w lokalu"),
            LoyaltyPointLog.Create(customers.Sakura.Id, 75, "Zakup online"),
            LoyaltyPointLog.Create(customers.Sakura.Id, 100, "Udział w wydarzeniu"),
            LoyaltyPointLog.Create(customers.Sakura.Id, -150, "Wykorzystanie punktów"));

        db.LoyaltyPointLogs.Add(
            LoyaltyPointLog.Create(customers.Yuki.Id, 30, "Pierwsze zamówienie"));

        await db.SaveChangesAsync();
    }

    // === Events ===

    private async Task SeedEventsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var matchaTasting = Event.Create(
            "Matcha Tasting Workshop",
            description: "Wieczór degustacji matchy z trzech regionów Japonii.",
            imageUrl: null);
        db.Events.Add(matchaTasting);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(matchaTasting.Id, DateOnly.FromDateTime(now.AddDays(-14).Date)));
        matchaTasting.Publish(now.AddDays(-21));
        matchaTasting.Close(now.AddDays(-13));

        var sakuraHanami = Event.Create(
            "Sakura Hanami Afternoon",
            description: "Popołudnie pod kwitnącymi wiśniami z wagashi i herbatą.",
            imageUrl: null);
        db.Events.Add(sakuraHanami);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(sakuraHanami.Id, new DateOnly(2026, 5, 18)));
        sakuraHanami.Publish(now.AddDays(-2));

        var wagashiClass = Event.Create(
            "Wagashi Making Class",
            description: "Warsztaty robienia wagashi pod okiem zaproszonej cukierniczki.",
            imageUrl: null);
        db.Events.Add(wagashiClass);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(wagashiClass.Id, new DateOnly(2026, 6, 14)));

        await db.SaveChangesAsync();
    }

    // === Promotion codes ===

    public sealed record PromotionRefs(PromotionCode Welcome10, PromotionCode Sakura2026, PromotionCode Hanami);

    private async Task<PromotionRefs> SeedPromotionCodesAsync()
    {
        var welcome10 = PromotionCode.Create("WITAJ10", 10m, maxUses: 1);
        var sakura2026 = PromotionCode.Create("SAKURA2026", 15m,
            validUntil: new DateTimeOffset(2026, 5, 31, 23, 59, 59, TimeSpan.Zero));
        var hanami = PromotionCode.Create("HANAMI", 15m,
            validFrom: new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero),
            validUntil: new DateTimeOffset(2026, 5, 31, 23, 59, 59, TimeSpan.Zero));
        db.PromotionCodes.AddRange(welcome10, sakura2026, hanami);
        await db.SaveChangesAsync();
        return new PromotionRefs(welcome10, sakura2026, hanami);
    }

    // === Sample orders ===

    private async Task SeedSampleOrdersAsync(
        Guid outletId,
        ProductRefs products,
        CustomerRefs customers,
        TableRefs tables,
        SalesChannelRefs salesChannels,
        PromotionRefs promotions)
    {
        var now = DateTimeOffset.UtcNow;

        var dineIn = Order.Create(outletId, tableId: tables.Bar1.Id, salesChannelId: salesChannels.Pos.Id, userId: customers.Sakura.Id);
        db.Orders.Add(dineIn);
        await db.SaveChangesAsync();
        AddOrderLine(dineIn.Id, products.Cappuccino, 2, 14.00m, 0.08m);
        AddOrderLine(dineIn.Id, products.IchigoShortcake, 1, 22.00m, 0.08m);
        dineIn.Close(now);

        var takeaway = Order.Create(outletId, salesChannelId: salesChannels.Online.Id, userId: customers.Yuki.Id);
        db.Orders.Add(takeaway);
        await db.SaveChangesAsync();
        AddOrderLine(takeaway.Id, products.YuzuLemoniada, 1, 16.00m, 0.05m);
        AddOrderLine(takeaway.Id, products.OnigiriSalmon, 2, 12.00m, 0.05m);
        takeaway.Close(now.AddHours(-1));

        var pickup = Order.Create(outletId, salesChannelId: salesChannels.Mobile.Id, userId: customers.Hana.Id);
        db.Orders.Add(pickup);
        await db.SaveChangesAsync();
        AddOrderLine(pickup.Id, products.MatchaLatte, 1, 18.00m, 0.05m);
        AddOrderLine(pickup.Id, products.Dorayaki, 1, 10.00m, 0.05m);

        var cancelled = Order.Create(outletId, tableId: tables.Window2.Id, salesChannelId: salesChannels.Pos.Id);
        db.Orders.Add(cancelled);
        await db.SaveChangesAsync();
        AddOrderLine(cancelled.Id, products.Espresso, 1, 9.00m, 0.08m);
        cancelled.Cancel(now.AddHours(-3), "Klient nie wrócił po napój.");

        var promoOrder = Order.Create(outletId, tableId: tables.Tatami.Id, salesChannelId: salesChannels.Pos.Id,
            userId: customers.Sakura.Id, loyaltyPointsUsed: 50);
        db.Orders.Add(promoOrder);
        await db.SaveChangesAsync();
        AddOrderLine(promoOrder.Id, products.HojichaLatte, 1, 18.00m, 0.08m);
        AddOrderLine(promoOrder.Id, products.MatchaTiramisu, 1, 24.00m, 0.08m);
        promoOrder.AssignPromotion(promotions.Welcome10.Id, discount: 4.20m);
        promotions.Welcome10.RegisterUsage();
        promoOrder.Close(now.AddMinutes(-30));

        await db.SaveChangesAsync();
    }

    private void AddOrderLine(Guid orderId, Product product, int quantity, decimal netPerOne, decimal vatRate)
    {
        var vatPerOne = Math.Round(netPerOne * vatRate, 2);
        db.OrderLines.Add(OrderLine.Create(orderId, product.Id, quantity, netPerOne, vatPerOne));
    }

    // === Printout templates ===

    private async Task SeedPrintoutTemplatesAsync()
    {
        db.PrintoutTemplates.AddRange(
            PrintoutTemplate.Create("Paragon", "/templates/paragon.docx"),
            PrintoutTemplate.Create("Bonik dla kuchni", "/templates/bonik-kuchnia.docx"),
            PrintoutTemplate.Create("Potwierdzenie wydarzenia", "/templates/potwierdzenie-wydarzenia.docx"));
        await db.SaveChangesAsync();
    }

    // === Backdate audit timestamps for sortable variety ===

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
                    created_by = (ARRAY[{0}::uuid, {1}::uuid, {2}::uuid, {3}::uuid])[((numbered.rn - 1) % 4 + 1)::int]
                FROM numbered
                WHERE t.id = numbered.id;
                """;

            await db.Database.ExecuteSqlRawAsync(
                sql,
                creatorUserIds[0], creatorUserIds[1], creatorUserIds[2], creatorUserIds[3]);
        }
    }
}
