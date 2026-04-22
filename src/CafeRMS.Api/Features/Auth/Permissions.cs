namespace CafeRMS.Api.Features.Auth;

public static class Permissions
{
    public const string OutletManage = "OutletManage";
    public const string TablesManage = "TablesManage";
    public const string ProductsManage = "ProductsManage";
    public const string ModifiersManage = "ModifiersManage";
    public const string MenusManage = "MenusManage";
    public const string TaxRatesManage = "TaxRatesManage";
    public const string PricingManage = "PricingManage";
    public const string SalesChannelsManage = "SalesChannelsManage";
    public const string PromotionsManage = "PromotionsManage";
    public const string EventsManage = "EventsManage";
    public const string UsersManage = "UsersManage";
    public const string RolesManage = "RolesManage";
    public const string PrintoutTemplatesManage = "PrintoutTemplatesManage";
    public const string OrdersView = "OrdersView";
    public const string OrdersManage = "OrdersManage";
    public const string ReportsView = "ReportsView";
    public const string LoyaltyManage = "LoyaltyManage";

    public static readonly string[] All =
    [
        OutletManage, TablesManage, ProductsManage, ModifiersManage,
        MenusManage, TaxRatesManage, PricingManage, SalesChannelsManage,
        PromotionsManage, EventsManage, UsersManage, RolesManage,
        PrintoutTemplatesManage, OrdersView, OrdersManage, ReportsView,
        LoyaltyManage
    ];
}
