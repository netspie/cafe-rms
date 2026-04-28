using System.Security.Claims;
using CafeRMS.Api.Features.Allergens;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies;
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

// Whole-cafe demo seed. Replaces the bland "Demo Cafe" with a themed Japanese café
// in Warsaw — reference data, pricing, ~30 products, staff, customers, loyalty,
// events, promotions, sample orders. Idempotent: looks up the Yumeya company by
// its legal name and bails out if it's already there.
//
// Bypasses the company-scoped global query filter via db.SetCompanyContext(...)
// after provisioning, so every subsequent query and add is naturally scoped to
// Yumeya without any IgnoreQueryFilters scattered around.
public class YumeyaDemoSeeder(
    AppDbContext db,
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IConfiguration config,
    ILogger<YumeyaDemoSeeder> logger)
{
    private const string CompanyLegalName = "Yumeya Sp. z o.o.";
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

        var isAlreadySeeded = await db.Companies.AnyAsync(x => x.LegalName == CompanyLegalName);
        if (isAlreadySeeded)
            return;

        var provisioned = await ProvisionAsync(ownerPassword);
        db.SetCompanyContext(provisioned.CompanyId);

        var taxRates = await SeedTaxRatesAsync(provisioned.CompanyId);
        var tags = await SeedTagsAsync(provisioned.CompanyId);
        var allergens = await SeedAllergensAsync(provisioned.CompanyId);
        var modifierGroups = await SeedModifierGroupsAsync(provisioned.CompanyId);
        var priceGroups = await SeedPriceGroupsAsync(provisioned.CompanyId);
        var salesChannels = await SeedSalesChannelsAsync(provisioned.CompanyId, priceGroups);
        var tables = await SeedTablesAsync(provisioned.CompanyId, provisioned.OutletId);
        var products = await SeedProductsAsync(provisioned.CompanyId, taxRates, tags, allergens, modifierGroups, priceGroups);
        await SeedStaffAsync(provisioned.CompanyId, ownerPassword);
        var customers = await SeedCustomersAsync(ownerPassword);
        await SeedLoyaltyAsync(provisioned.CompanyId, customers);
        await SeedEventsAsync(provisioned.CompanyId);
        var promotions = await SeedPromotionCodesAsync(provisioned.CompanyId);
        await SeedSampleOrdersAsync(provisioned.OutletId, products, customers, tables, salesChannels, promotions);
        await SeedPrintoutTemplatesAsync(provisioned.CompanyId);

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Seeded Yumeya demo company {LegalName} (CompanyId={CompanyId}, OutletId={OutletId}, OwnerUserId={OwnerUserId})",
            CompanyLegalName, provisioned.CompanyId, provisioned.OutletId, provisioned.OwnerUserId);
    }

    private Task<CompanyOwnerProvisioningResult> ProvisionAsync(string ownerPassword)
    {
        var input = new CompanyOwnerProvisioningInput(
            LegalName: CompanyLegalName,
            TaxId: "5252525252",
            InvoicingAddress: OutletAddress,
            BillingEmail: "biuro@yumeya.pl",
            BillingPhone: "+48221234567",
            IsPublic: true,
            OutletDisplayName: OutletDisplayName,
            OutletStreetAddress: OutletAddress,
            OutletPhone: "+48221234567",
            OutletTimeZone: "Europe/Warsaw",
            OutletCurrency: Currency.PLN,
            OutletLogoUrl: null,
            OwnerEmail: OwnerEmail,
            OwnerPassword: ownerPassword,
            OwnerFirstName: "Dariusz",
            OwnerLastName: "Luśnia");

        return CompanyProvisioning.CreateCompanyWithOwnerAsync(input, userManager, roleManager, db);
    }

    // === Reference data ===

    public sealed record TaxRateRefs(TaxRate EatIn, TaxRate Takeaway, TaxRate Goods);

    private async Task<TaxRateRefs> SeedTaxRatesAsync(Guid companyId)
    {
        var eatIn = TaxRate.Create("VAT 8%", "Gastronomia na miejscu", 8m, companyId);
        var takeaway = TaxRate.Create("VAT 5%", "Żywność na wynos", 5m, companyId);
        var goods = TaxRate.Create("VAT 23%", "Towary (kubki, torby)", 23m, companyId);
        db.TaxRates.AddRange(eatIn, takeaway, goods);
        await db.SaveChangesAsync();
        return new TaxRateRefs(eatIn, takeaway, goods);
    }

    public sealed record TagRefs(
        Tag HotDrinks, Tag IcedDrinks, Tag Coffee, Tag MatchaTea,
        Tag Cakes, Tag Wagashi, Tag Savoury,
        Tag Seasonal, Tag Vegan, Tag GlutenFree);

    private async Task<TagRefs> SeedTagsAsync(Guid companyId)
    {
        var hotDrinks = Tag.Create("Napoje gorące", companyId);
        var icedDrinks = Tag.Create("Napoje zimne", companyId);
        var coffee = Tag.Create("Kawa", companyId);
        var matchaTea = Tag.Create("Matcha & herbata", companyId);
        var cakes = Tag.Create("Ciasta", companyId);
        var wagashi = Tag.Create("Wagashi", companyId);
        var savoury = Tag.Create("Przekąski", companyId);
        var seasonal = Tag.Create("Sezonowe", companyId);
        var vegan = Tag.Create("Wegańskie", companyId);
        var glutenFree = Tag.Create("Bezglutenowe", companyId);
        db.Tags.AddRange(hotDrinks, icedDrinks, coffee, matchaTea, cakes, wagashi, savoury, seasonal, vegan, glutenFree);
        await db.SaveChangesAsync();
        return new TagRefs(hotDrinks, icedDrinks, coffee, matchaTea, cakes, wagashi, savoury, seasonal, vegan, glutenFree);
    }

    public sealed record AllergenRefs(
        Allergen Milk, Allergen Egg, Allergen Gluten, Allergen Soy,
        Allergen Sesame, Allergen TreeNuts, Allergen Peanuts);

    private async Task<AllergenRefs> SeedAllergensAsync(Guid companyId)
    {
        var milk = Allergen.Create("Mleko", companyId);
        var egg = Allergen.Create("Jajka", companyId);
        var gluten = Allergen.Create("Gluten", companyId);
        var soy = Allergen.Create("Soja", companyId);
        var sesame = Allergen.Create("Sezam", companyId);
        var treeNuts = Allergen.Create("Orzechy", companyId);
        var peanuts = Allergen.Create("Orzeszki ziemne", companyId);
        db.Allergens.AddRange(milk, egg, gluten, soy, sesame, treeNuts, peanuts);
        await db.SaveChangesAsync();
        return new AllergenRefs(milk, egg, gluten, soy, sesame, treeNuts, peanuts);
    }

    public sealed record ModifierGroupRefs(
        ModifierGroup Size, ModifierGroup Milk, ModifierGroup Sweetness,
        ModifierGroup Temperature, ModifierGroup ExtraShot,
        ModifierGroup WhippedCream, ModifierGroup ExtraSweet);

    private async Task<ModifierGroupRefs> SeedModifierGroupsAsync(Guid companyId)
    {
        var size = ModifierGroup.Create("Rozmiar", companyId);
        var milk = ModifierGroup.Create("Mleko", companyId);
        var sweetness = ModifierGroup.Create("Słodkość", companyId);
        var temperature = ModifierGroup.Create("Temperatura", companyId);
        var extraShot = ModifierGroup.Create("Dodatkowy shot", companyId);
        var whippedCream = ModifierGroup.Create("Bita śmietana", companyId);
        var extraSweet = ModifierGroup.Create("Dodatki deserowe", companyId);

        db.ModifierGroups.AddRange(size, milk, sweetness, temperature, extraShot, whippedCream, extraSweet);
        await db.SaveChangesAsync();

        // Modifier options. No price impact — modifiers are pure choices, the line
        // price comes from the product alone.
        db.Modifiers.AddRange(
            Modifier.Create("Standard", size.Id, companyId),
            Modifier.Create("Duży", size.Id, companyId),

            Modifier.Create("Krowie", milk.Id, companyId),
            Modifier.Create("Owsiane", milk.Id, companyId),
            Modifier.Create("Sojowe", milk.Id, companyId),
            Modifier.Create("Migdałowe", milk.Id, companyId),

            Modifier.Create("Brak", sweetness.Id, companyId),
            Modifier.Create("Mała", sweetness.Id, companyId),
            Modifier.Create("Standardowa", sweetness.Id, companyId),
            Modifier.Create("Większa", sweetness.Id, companyId),

            Modifier.Create("Gorące", temperature.Id, companyId),
            Modifier.Create("Mrożone", temperature.Id, companyId),

            Modifier.Create("Bez", extraShot.Id, companyId),
            Modifier.Create("Dodatkowy shot", extraShot.Id, companyId),

            Modifier.Create("Bez", whippedCream.Id, companyId),
            Modifier.Create("Z bitą śmietaną", whippedCream.Id, companyId),

            Modifier.Create("Bez dodatków", extraSweet.Id, companyId),
            Modifier.Create("Dodatkowe anko", extraSweet.Id, companyId),
            Modifier.Create("Dodatkowe mochi", extraSweet.Id, companyId));

        await db.SaveChangesAsync();
        return new ModifierGroupRefs(size, milk, sweetness, temperature, extraShot, whippedCream, extraSweet);
    }

    // === Pricing ===

    public sealed record PriceGroupRefs(PriceGroup Standard, PriceGroup Loyalty, PriceGroup HappyHour);

    private async Task<PriceGroupRefs> SeedPriceGroupsAsync(Guid companyId)
    {
        var standard = PriceGroup.Create("Standardowa", companyId);
        var loyalty = PriceGroup.Create("Lojalność", companyId);
        var happyHour = PriceGroup.Create("Happy hour", companyId);
        db.PriceGroups.AddRange(standard, loyalty, happyHour);
        await db.SaveChangesAsync();
        return new PriceGroupRefs(standard, loyalty, happyHour);
    }

    public sealed record SalesChannelRefs(SalesChannel Pos, SalesChannel Online, SalesChannel Mobile);

    private async Task<SalesChannelRefs> SeedSalesChannelsAsync(Guid companyId, PriceGroupRefs priceGroups)
    {
        var pos = SalesChannel.Create("Lokal (POS)", isTakeout: false, companyId);
        var online = SalesChannel.Create("Online", isTakeout: true, companyId);
        var mobile = SalesChannel.Create("Aplikacja mobilna", isTakeout: true, companyId);
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

    private async Task<TableRefs> SeedTablesAsync(Guid companyId, Guid outletId)
    {
        var bar1 = Table.Create("Bar 1", outletId, companyId);
        var bar2 = Table.Create("Bar 2", outletId, companyId);
        var bar3 = Table.Create("Bar 3", outletId, companyId);
        var window1 = Table.Create("Okno 1", outletId, companyId);
        var window2 = Table.Create("Okno 2", outletId, companyId);
        var tatami = Table.Create("Tatami", outletId, companyId);
        var patio1 = Table.Create("Ogródek 1", outletId, companyId);
        var patio2 = Table.Create("Ogródek 2", outletId, companyId);
        db.Tables.AddRange(bar1, bar2, bar3, window1, window2, tatami, patio1, patio2);
        await db.SaveChangesAsync();
        return new TableRefs(bar1, bar2, bar3, window1, window2, tatami, patio1, patio2);
    }

    // === Products ===
    //
    // 30 products spread across drinks / cakes / wagashi / savoury / merch.
    // Each product gets:
    //   * one tax rate (eat-in for consumables, goods for merch)
    //   * a few tags (filterable categories on the menu)
    //   * declared allergens (informative, not blocking)
    //   * attached modifier groups (size / milk / sweetness / etc.)
    //   * three prices — one per price group — Standard, Loyalty (-10%), HappyHour (-15%)
    //
    // The prices are computed from a single base value via Discount(...) so the seed
    // stays compact and the math is easy to walk through during defense.

    public sealed record ProductRefs(
        Product MatchaLatte, Product HojichaLatte, Product KuroGomaLatte, Product SakuraLatte,
        Product YuzuLemoniada, Product UmeSoda, Product IchigoMilk,
        Product KawaParzona, Product Espresso, Product Cappuccino, Product EarlGrey, Product Genmaicha,
        Product IchigoShortcake, Product MatchaTiramisu, Product YuzuBasque, Product MontBlanc,
        Product HojichaRoll, Product KuroGomaOpera,
        Product MochiDonut, Product IchigoDaifuku, Product Dorayaki, Product SakuraMochi,
        Product TamagoSando, Product KatsuSando, Product OnigiriSalmon, Product OnigiriUmeboshi, Product ZupaMiso);

    private async Task<ProductRefs> SeedProductsAsync(
        Guid companyId,
        TaxRateRefs taxRates,
        TagRefs tags,
        AllergenRefs allergens,
        ModifierGroupRefs modifierGroups,
        PriceGroupRefs priceGroups)
    {
        // Drinks — eat-in VAT (8%)
        var matchaLatte = AddProduct(companyId, "Matcha latte", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea, tags.Vegan],
            [allergens.Soy],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature, modifierGroups.ExtraShot, modifierGroups.WhippedCream]);
        var hojichaLatte = AddProduct(companyId, "Hojicha latte", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var kuroGomaLatte = AddProduct(companyId, "Kuro goma latte", taxRates.EatIn.Id, 19.00m, priceGroups,
            [tags.HotDrinks],
            [allergens.Milk, allergens.Sesame],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var sakuraLatte = AddProduct(companyId, "Sakura latte", taxRates.EatIn.Id, 20.00m, priceGroups,
            [tags.HotDrinks, tags.Seasonal],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var yuzuLemoniada = AddProduct(companyId, "Yuzu lemoniada", taxRates.EatIn.Id, 16.00m, priceGroups,
            [tags.IcedDrinks, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness]);
        var umeSoda = AddProduct(companyId, "Ume soda", taxRates.EatIn.Id, 16.00m, priceGroups,
            [tags.IcedDrinks, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness]);
        var ichigoMilk = AddProduct(companyId, "Ichigo milk", taxRates.EatIn.Id, 17.00m, priceGroups,
            [tags.IcedDrinks],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var kawaParzona = AddProduct(companyId, "Kawa parzona", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.HotDrinks, tags.Coffee, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var espresso = AddProduct(companyId, "Espresso", taxRates.EatIn.Id, 9.00m, priceGroups,
            [tags.HotDrinks, tags.Coffee, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.ExtraShot]);
        var cappuccino = AddProduct(companyId, "Cappuccino", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.HotDrinks, tags.Coffee],
            [allergens.Milk],
            [modifierGroups.Size, modifierGroups.Milk, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var earlGrey = AddProduct(companyId, "Earl grey", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness, modifierGroups.Temperature]);
        var genmaicha = AddProduct(companyId, "Genmaicha", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.HotDrinks, tags.MatchaTea, tags.Vegan, tags.GlutenFree],
            [],
            [modifierGroups.Size, modifierGroups.Sweetness, modifierGroups.Temperature]);

        // Cakes & desserts — eat-in VAT
        var ichigoShortcake = AddProduct(companyId, "Ichigo shortcake", taxRates.EatIn.Id, 22.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            [modifierGroups.WhippedCream, modifierGroups.ExtraSweet]);
        var matchaTiramisu = AddProduct(companyId, "Matcha tiramisu", taxRates.EatIn.Id, 24.00m, priceGroups,
            [tags.Cakes, tags.MatchaTea],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            [modifierGroups.ExtraSweet]);
        var yuzuBasque = AddProduct(companyId, "Yuzu basque cheesecake", taxRates.EatIn.Id, 26.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg],
            [modifierGroups.ExtraSweet]);
        var montBlanc = AddProduct(companyId, "Mont Blanc", taxRates.EatIn.Id, 28.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten, allergens.TreeNuts],
            [modifierGroups.WhippedCream]);
        var hojichaRoll = AddProduct(companyId, "Hojicha roll cake", taxRates.EatIn.Id, 22.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten],
            [modifierGroups.WhippedCream]);
        var kuroGomaOpera = AddProduct(companyId, "Kuro goma opera", taxRates.EatIn.Id, 26.00m, priceGroups,
            [tags.Cakes],
            [allergens.Milk, allergens.Egg, allergens.Gluten, allergens.Sesame],
            [modifierGroups.ExtraSweet]);
        var mochiDonut = AddProduct(companyId, "Mochi donut", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.Cakes, tags.Wagashi],
            [allergens.Gluten, allergens.Milk],
            [modifierGroups.Sweetness]);
        var ichigoDaifuku = AddProduct(companyId, "Ichigo daifuku", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.Wagashi, tags.GlutenFree],
            [],
            [modifierGroups.ExtraSweet]);
        var dorayaki = AddProduct(companyId, "Dorayaki", taxRates.EatIn.Id, 10.00m, priceGroups,
            [tags.Wagashi],
            [allergens.Egg, allergens.Gluten],
            [modifierGroups.ExtraSweet]);
        var sakuraMochi = AddProduct(companyId, "Sakura mochi", taxRates.EatIn.Id, 11.00m, priceGroups,
            [tags.Wagashi, tags.Seasonal, tags.GlutenFree],
            [],
            [modifierGroups.ExtraSweet]);

        // Savoury — eat-in VAT
        var tamagoSando = AddProduct(companyId, "Tamago sando", taxRates.EatIn.Id, 18.00m, priceGroups,
            [tags.Savoury],
            [allergens.Egg, allergens.Gluten, allergens.Milk],
            []);
        var katsuSando = AddProduct(companyId, "Mini katsu sando", taxRates.EatIn.Id, 22.00m, priceGroups,
            [tags.Savoury],
            [allergens.Gluten, allergens.Milk, allergens.Soy],
            []);
        var onigiriSalmon = AddProduct(companyId, "Onigiri łosoś", taxRates.EatIn.Id, 12.00m, priceGroups,
            [tags.Savoury, tags.GlutenFree],
            [],
            []);
        var onigiriUmeboshi = AddProduct(companyId, "Onigiri umeboshi", taxRates.EatIn.Id, 10.00m, priceGroups,
            [tags.Savoury, tags.Vegan, tags.GlutenFree],
            [],
            []);
        var zupaMiso = AddProduct(companyId, "Zupa miso", taxRates.EatIn.Id, 14.00m, priceGroups,
            [tags.Savoury, tags.Vegan, tags.GlutenFree],
            [allergens.Soy],
            [modifierGroups.Temperature]);

        // Merch — goods VAT (23%)
        AddProduct(companyId, "Kubek emaliowany Yumeya", taxRates.Goods.Id, 65.00m, priceGroups, [], [], []);
        AddProduct(companyId, "Furoshiki", taxRates.Goods.Id, 95.00m, priceGroups, [], [], []);
        AddProduct(companyId, "Puszka matcha 50g", taxRates.Goods.Id, 110.00m, priceGroups, [], [], []);

        await db.SaveChangesAsync();

        // Single product list "Menu główne" — every consumable, in roughly the order
        // a customer would scan a menu (drinks → cakes → wagashi → savoury). Merch
        // intentionally excluded so it doesn't show up alongside the food.
        var mainMenu = ProductList.Create("Menu główne", companyId);
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

    // Builds a Product + its tag / allergen / modifier-group links + three price rows
    // (Standard, Loyalty -10%, HappyHour -15%) and stages everything on the change
    // tracker. Caller invokes SaveChangesAsync once when the whole batch is built.
    private Product AddProduct(
        Guid companyId,
        string name,
        Guid taxRateId,
        decimal standardNet,
        PriceGroupRefs priceGroups,
        Tag[] productTags,
        Allergen[] productAllergens,
        ModifierGroup[] productModifierGroups)
    {
        var product = Product.Create(name, taxRateId, companyId);
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

    // === Staff: roles + permission claims + users ===
    //
    // CompanyProvisioning already created the Owner role + the dariusz@yumeya.pl
    // owner. Here we add the rest of the org chart:
    //
    //   * Manager  — broad access (everything except role admin)
    //   * Barista  — orders, loyalty, edits to the menu
    //   * Kuchnia  — orders view only
    //
    // Owner needs no permission claims — PermissionAuthorizationHandler short-circuits
    // when the user is in SystemRoles.Owner. Other roles get explicit RoleClaim rows
    // (one per permission), exactly matching the runtime CreateRole / UpdateRole flow.

    private async Task SeedStaffAsync(Guid companyId, string staffPassword)
    {
        var managerRole = await CreateRoleWithPermissionsAsync(companyId, "Kierownik",
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
        var baristaRole = await CreateRoleWithPermissionsAsync(companyId, "Barista",
        [
            Permissions.OrdersView, Permissions.OrdersManage,
            Permissions.LoyaltyManage, Permissions.ProductsManage
        ]);
        var kitchenRole = await CreateRoleWithPermissionsAsync(companyId, "Kuchnia",
        [
            Permissions.OrdersView
        ]);

        await CreateStaffAsync("kierownik@yumeya.pl", "Maja", "Kowalska", staffPassword, companyId, managerRole.Id);
        await CreateStaffAsync("anna@yumeya.pl", "Anna", "Nowak", staffPassword, companyId, baristaRole.Id);
        await CreateStaffAsync("kenji@yumeya.pl", "Kenji", "Tanaka", staffPassword, companyId, baristaRole.Id);
        await CreateStaffAsync("yuna@yumeya.pl", "Yuna", "Kim", staffPassword, companyId, kitchenRole.Id);
    }

    private async Task<AppRole> CreateRoleWithPermissionsAsync(Guid companyId, string roleName, string[] permissions)
    {
        var role = AppRole.Create(roleName, companyId);
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

    private async Task CreateStaffAsync(string email, string firstName, string lastName, string password, Guid companyId, Guid roleId)
    {
        var user = AppUser.Create(email, firstName, lastName, AccountType.Staff, companyId);
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new DomainException(string.Join("; ", createResult.Errors.Select(x => x.Description)));

        // Same trick as CompanyProvisioning — add the user-role link directly so it
        // doesn't fight the AppRole global query filter.
        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = roleId });
        await db.SaveChangesAsync();
    }

    // === Customers (loyalty members) ===

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

    // === Loyalty point history ===

    private async Task SeedLoyaltyAsync(Guid companyId, CustomerRefs customers)
    {
        // Sakura — Złoty: three earn entries + one redemption (balance ≈ 75 pts).
        db.LoyaltyPointLogs.AddRange(
            LoyaltyPointLog.Create(customers.Sakura.Id, 50, companyId, "Zakup w lokalu"),
            LoyaltyPointLog.Create(customers.Sakura.Id, 75, companyId, "Zakup online"),
            LoyaltyPointLog.Create(customers.Sakura.Id, 100, companyId, "Udział w wydarzeniu"),
            LoyaltyPointLog.Create(customers.Sakura.Id, -150, companyId, "Wykorzystanie punktów"));

        // Yuki — Srebrny: a single earn.
        db.LoyaltyPointLogs.Add(
            LoyaltyPointLog.Create(customers.Yuki.Id, 30, companyId, "Pierwsze zamówienie"));

        // Hana — fresh, no entries.
        await db.SaveChangesAsync();
    }

    // === Events (covers the Phase 4 Events lifecycle) ===

    private async Task SeedEventsAsync(Guid companyId)
    {
        var now = DateTimeOffset.UtcNow;

        // Past, closed event — Matcha Tasting Workshop, two weeks ago.
        var matchaTasting = Event.Create(
            "Matcha Tasting Workshop",
            companyId,
            description: "Wieczór degustacji matchy z trzech regionów Japonii.",
            imageUrl: null);
        db.Events.Add(matchaTasting);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(matchaTasting.Id, DateOnly.FromDateTime(now.AddDays(-14).Date)));
        matchaTasting.Publish(now.AddDays(-21));
        matchaTasting.Close(now.AddDays(-13));

        // Upcoming, published event — Sakura Hanami Afternoon, next month.
        var sakuraHanami = Event.Create(
            "Sakura Hanami Afternoon",
            companyId,
            description: "Popołudnie pod kwitnącymi wiśniami z wagashi i herbatą.",
            imageUrl: null);
        db.Events.Add(sakuraHanami);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(sakuraHanami.Id, new DateOnly(2026, 5, 18)));
        sakuraHanami.Publish(now.AddDays(-2));

        // Draft event — Wagashi Making Class, June.
        var wagashiClass = Event.Create(
            "Wagashi Making Class",
            companyId,
            description: "Warsztaty robienia wagashi pod okiem zaproszonej cukierniczki.",
            imageUrl: null);
        db.Events.Add(wagashiClass);
        await db.SaveChangesAsync();
        db.EventDays.Add(EventDay.Create(wagashiClass.Id, new DateOnly(2026, 6, 14)));

        await db.SaveChangesAsync();
    }

    // === Promotion codes ===

    public sealed record PromotionRefs(PromotionCode Welcome10, PromotionCode Sakura2026, PromotionCode Hanami);

    private async Task<PromotionRefs> SeedPromotionCodesAsync(Guid companyId)
    {
        var welcome10 = PromotionCode.Create("WITAJ10", 10m, companyId, maxUses: 1);
        var sakura2026 = PromotionCode.Create("SAKURA2026", 15m, companyId,
            validUntil: new DateTimeOffset(2026, 5, 31, 23, 59, 59, TimeSpan.Zero));
        var hanami = PromotionCode.Create("HANAMI", 15m, companyId,
            validFrom: new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero),
            validUntil: new DateTimeOffset(2026, 5, 31, 23, 59, 59, TimeSpan.Zero));
        db.PromotionCodes.AddRange(welcome10, sakura2026, hanami);
        await db.SaveChangesAsync();
        return new PromotionRefs(welcome10, sakura2026, hanami);
    }

    // === Sample orders ===
    //
    // Five orders covering the lifecycle states the list filter can show:
    //   1. Closed dine-in            — Sakura at the bar, paid + closed today
    //   2. Closed takeaway           — Yuki via Online channel, picked up
    //   3. Placed pickup             — Hana via Mobile, still waiting
    //   4. Cancelled                 — POS walk-in cancelled by manager
    //   5. Closed dine-in with promo — Sakura at Tatami, WITAJ10 applied
    //
    // VAT-per-line is the eat-in 8 % rate (or takeaway 5 %) computed from the line's
    // standard net price — matches what runtime PlaceOrder writes to OrderLine.

    private async Task SeedSampleOrdersAsync(
        Guid outletId,
        ProductRefs products,
        CustomerRefs customers,
        TableRefs tables,
        SalesChannelRefs salesChannels,
        PromotionRefs promotions)
    {
        var now = DateTimeOffset.UtcNow;

        // 1. Closed dine-in — Sakura, two coffees + a cake.
        var dineIn = Order.Create(outletId, tableId: tables.Bar1.Id, salesChannelId: salesChannels.Pos.Id, userId: customers.Sakura.Id);
        db.Orders.Add(dineIn);
        await db.SaveChangesAsync();
        AddOrderLine(dineIn.Id, products.Cappuccino, 2, 14.00m, 0.08m);
        AddOrderLine(dineIn.Id, products.IchigoShortcake, 1, 22.00m, 0.08m);
        dineIn.Close(now);

        // 2. Closed takeaway — Yuki, lemoniada + onigiri via Online channel.
        var takeaway = Order.Create(outletId, salesChannelId: salesChannels.Online.Id, userId: customers.Yuki.Id);
        db.Orders.Add(takeaway);
        await db.SaveChangesAsync();
        AddOrderLine(takeaway.Id, products.YuzuLemoniada, 1, 16.00m, 0.05m);
        AddOrderLine(takeaway.Id, products.OnigiriSalmon, 2, 12.00m, 0.05m);
        takeaway.Close(now.AddHours(-1));

        // 3. Placed pickup — Hana, mobile order in flight.
        var pickup = Order.Create(outletId, salesChannelId: salesChannels.Mobile.Id, userId: customers.Hana.Id);
        db.Orders.Add(pickup);
        await db.SaveChangesAsync();
        AddOrderLine(pickup.Id, products.MatchaLatte, 1, 18.00m, 0.05m);
        AddOrderLine(pickup.Id, products.Dorayaki, 1, 10.00m, 0.05m);

        // 4. Cancelled — walk-in POS that didn't pay.
        var cancelled = Order.Create(outletId, tableId: tables.Window2.Id, salesChannelId: salesChannels.Pos.Id);
        db.Orders.Add(cancelled);
        await db.SaveChangesAsync();
        AddOrderLine(cancelled.Id, products.Espresso, 1, 9.00m, 0.08m);
        cancelled.Cancel(now.AddHours(-3), "Klient nie wrócił po napój.");

        // 5. Closed dine-in with WITAJ10 — Sakura at Tatami, 10 % off the line totals.
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
    //
    // Three placeholder templates so the Printouts admin page isn't empty. Phase 8
    // wires real .docx files through the rendering engine — for now the URL points
    // at a static asset that doesn't exist yet. The metadata is what matters here.

    private async Task SeedPrintoutTemplatesAsync(Guid companyId)
    {
        db.PrintoutTemplates.AddRange(
            PrintoutTemplate.Create("Paragon", "/templates/paragon.docx", companyId),
            PrintoutTemplate.Create("Bonik dla kuchni", "/templates/bonik-kuchnia.docx", companyId),
            PrintoutTemplate.Create("Potwierdzenie wydarzenia", "/templates/potwierdzenie-wydarzenia.docx", companyId));
        await db.SaveChangesAsync();
    }
}
