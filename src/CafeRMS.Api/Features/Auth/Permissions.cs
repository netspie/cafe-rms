namespace CafeRMS.Api.Features.Auth;

public static class Permissions
{
    public const string ManageCompanies = "permissions:manage_companies";
    public const string ManageOutlets = "permissions:manage_outlets";
    public const string ManageTables = "permissions:manage_tables";
    public const string ManageProducts = "permissions:manage_products";
    public const string ManageModifiers = "permissions:manage_modifiers";
    public const string ManageMenus = "permissions:manage_menus";
    public const string ManageTaxRates = "permissions:manage_tax_rates";
    public const string ManagePricing = "permissions:manage_pricing";
    public const string ManageSalesChannels = "permissions:manage_sales_channels";
    public const string ManagePromotions = "permissions:manage_promotions";
    public const string ManageEvents = "permissions:manage_events";
    public const string ManageUsers = "permissions:manage_users";
    public const string ManageRoles = "permissions:manage_roles";
    public const string ManagePrintoutTemplates = "permissions:manage_printout_templates";
    public const string ViewOrders = "permissions:view_orders";
    public const string ManageOrders = "permissions:manage_orders";
    public const string ViewReports = "permissions:view_reports";
    public const string PlaceOrders = "permissions:place_orders";
    public const string ManageFavorites = "permissions:manage_favorites";
    public const string ManageLoyalty = "permissions:manage_loyalty";

    public static readonly string[] All =
    [
        ManageCompanies, ManageOutlets, ManageTables, ManageProducts,
        ManageModifiers, ManageMenus, ManageTaxRates, ManagePricing,
        ManageSalesChannels, ManagePromotions, ManageEvents, ManageUsers,
        ManageRoles, ManagePrintoutTemplates, ViewOrders, ManageOrders,
        ViewReports, PlaceOrders, ManageFavorites, ManageLoyalty
    ];
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Customer = "Customer";
}
