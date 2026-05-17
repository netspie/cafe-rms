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
    IConfiguration config,
    ILogger<ShibaDemoSeeder> logger)
{
    private const string OwnerEmail = "dariusz@shiba.pl";
    private const string OutletDisplayName = "Shiba Cafe Warszawa";
    private const string OutletAddress = "ul. Marszałkowska 100, 00-001 Warszawa";

    public async Task SeedAsync()
    {
        var ownerPassword = config["Seed:OwnerPassword"];
        if (string.IsNullOrWhiteSpace(ownerPassword))
        {
            logger.LogWarning("Seed:OwnerPassword not set; skipping Shiba demo seed.");
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

        await BackdateAuditsForVarietyAsync([ownerUserId, staff.ManagerUserId, staff.BaristaUserId]);

        logger.LogInformation(
            "Seeded Shiba demo (OutletId={OutletId}, OwnerUserId={OwnerUserId})",
            outletId, ownerUserId);
    }

    private async Task<(Guid OwnerUserId, Guid OutletId)> ProvisionAsync(string ownerPassword)
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
            Currency.PLN);
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

    public sealed record PriceGroupRefs(PriceGroup Standard, PriceGroup Loyalty);

    private async Task<PriceGroupRefs> SeedPriceGroupsAsync()
    {
        var standard = PriceGroup.Create("Standardowa");
        var loyalty = PriceGroup.Create("Lojalność");
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
            SalesChannelPriceGroup.Create(dineIn.Id, priceGroups.Loyalty.Id),
            SalesChannelPriceGroup.Create(takeout.Id, priceGroups.Standard.Id),
            SalesChannelPriceGroup.Create(takeout.Id, priceGroups.Loyalty.Id));

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
        Product OnigiriSalmon);

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

        AddProduct("Kubek emaliowany Shiba", taxRates.Goods.Id, 65.00m, priceGroups, [], [], []);

        await db.SaveChangesAsync();

        return new ProductRefs(matchaLatte, hojichaLatte, cappuccino, yuzuLemoniada, shibaCookie, mochiDonut, cheesecake, onigiriSalmon);
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
            ProductPrice.Create(product.Id, priceGroups.Loyalty.Id, Discount(basePrice, 0.10m)));

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

        var manager = await CreateStaffAsync("kierownik@shiba.pl", "Maja", "Kowalska", staffPassword, managerRole.Id);
        var barista = await CreateStaffAsync("barista@shiba.pl", "Anna", "Nowak", staffPassword, baristaRole.Id);

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

    public sealed record CustomerRefs(AppUser Sakura, AppUser Yuki);

    private async Task<CustomerRefs> SeedCustomersAsync(string password)
    {
        var sakura = await CreateGuestAsync("sakura@example.jp", "Sakura", "Yamamoto", password);
        var yuki = await CreateGuestAsync("yuki@example.jp", "Yuki", "Watanabe", password);
        return new CustomerRefs(sakura, yuki);
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
            LoyaltyPointLog.Create(customers.Sakura.Id, 50, "Zakup w lokalu"),
            LoyaltyPointLog.Create(customers.Sakura.Id, 100, "Udział w wydarzeniu"),
            LoyaltyPointLog.Create(customers.Sakura.Id, -80, "Wykorzystanie punktów"));

        db.LoyaltyPointLogs.Add(
            LoyaltyPointLog.Create(customers.Yuki.Id, 30, "Pierwsze zamówienie"));

        await db.SaveChangesAsync();
    }

    private async Task SeedEventsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var shibaMeetup = Event.Create(
            "Shiba Meet-up",
            description: "Niedzielne popołudnie z miłośnikami Shib — kawa, herbata i mnóstwo puszystych ogonków.",
            imageUrl: null);
        db.Events.Add(shibaMeetup);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(shibaMeetup.Id, DateOnly.FromDateTime(now.AddDays(-10).Date)));
        shibaMeetup.Publish(now.AddDays(-17));
        shibaMeetup.Close(now.AddDays(-9));

        var pawPainting = Event.Create(
            "Paw Painting Workshop",
            description: "Warsztaty malowania pamiątkowych odcisków łap Twojego pupila.",
            imageUrl: null);
        db.Events.Add(pawPainting);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(pawPainting.Id, new DateOnly(2026, 6, 14)));
        pawPainting.Publish(now.AddDays(-2));

        await db.SaveChangesAsync();
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
        PromotionRefs promotions)
    {
        var now = DateTimeOffset.UtcNow;

        var dineIn = Order.Create(outletId, tableId: tables.Bar.Id, salesChannelId: salesChannels.DineIn.Id, userId: customers.Sakura.Id);
        db.Orders.Add(dineIn);
        await db.SaveChangesAsync();
        AddOrderLine(dineIn.Id, products.Cappuccino, 2, 14.00m, 0.08m);
        AddOrderLine(dineIn.Id, products.Cheesecake, 1, 22.00m, 0.08m);
        dineIn.Close(now);

        var takeaway = Order.Create(outletId, salesChannelId: salesChannels.Takeout.Id, userId: customers.Yuki.Id);
        db.Orders.Add(takeaway);
        await db.SaveChangesAsync();
        AddOrderLine(takeaway.Id, products.YuzuLemoniada, 1, 16.00m, 0.05m);
        AddOrderLine(takeaway.Id, products.OnigiriSalmon, 2, 12.00m, 0.05m);
        takeaway.Close(now.AddHours(-1));

        var pickup = Order.Create(outletId, salesChannelId: salesChannels.Takeout.Id, userId: customers.Sakura.Id);
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
            userId: customers.Sakura.Id, loyaltyPointsUsed: 50);
        db.Orders.Add(promoOrder);
        await db.SaveChangesAsync();
        AddOrderLine(promoOrder.Id, products.HojichaLatte, 1, 18.00m, 0.08m);
        AddOrderLine(promoOrder.Id, products.Cheesecake, 1, 22.00m, 0.08m);
        promoOrder.AssignPromotion(promotions.Welcome10.Id, discount: 4.32m);
        promotions.Welcome10.RegisterUsage();
        promoOrder.Close(now.AddMinutes(-30));

        await db.SaveChangesAsync();
    }

    private void AddOrderLine(Guid orderId, Product product, int quantity, decimal netPerOne, decimal vatRate)
    {
        var vatPerOne = Math.Round(netPerOne * vatRate, 2);
        db.OrderLines.Add(OrderLine.Create(orderId, product.Id, quantity, netPerOne, vatPerOne));
    }

    private async Task SeedPrintoutTemplatesAsync()
    {
        db.PrintoutTemplates.Add(
            PrintoutTemplate.Create("Potwierdzenie wydarzenia", "/Resources/Templates/potwierdzenie-wydarzenia.docx"));
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
