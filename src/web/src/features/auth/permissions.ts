// Mirrors CafeRMS.Api.Features.Auth.Permissions — keep in sync with the C# constants.

export const ALL_PERMISSIONS = [
  "OutletManage",
  "TablesManage",
  "ProductsManage",
  "ModifiersManage",
  "MenusManage",
  "TaxRatesManage",
  "PricingManage",
  "SalesChannelsManage",
  "PromotionsManage",
  "EventsManage",
  "UsersManage",
  "RolesManage",
  "PrintoutTemplatesManage",
  "OrdersView",
  "OrdersManage",
  "ReportsView",
  "LoyaltyManage",
] as const

export type Permission = (typeof ALL_PERMISSIONS)[number]

export const PERMISSION_LABELS: Record<Permission, string> = {
  OutletManage: "Outlet — manage",
  TablesManage: "Tables — manage",
  ProductsManage: "Products — manage",
  ModifiersManage: "Modifiers — manage",
  MenusManage: "Menus — manage",
  TaxRatesManage: "Tax rates — manage",
  PricingManage: "Pricing — manage",
  SalesChannelsManage: "Sales channels — manage",
  PromotionsManage: "Promotions — manage",
  EventsManage: "Events — manage",
  UsersManage: "Users — manage",
  RolesManage: "Roles — manage",
  PrintoutTemplatesManage: "Printout templates — manage",
  OrdersView: "Orders — view",
  OrdersManage: "Orders — manage (close / cancel)",
  ReportsView: "Reports — view",
  LoyaltyManage: "Loyalty — manage",
}
