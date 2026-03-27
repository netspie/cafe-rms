#region Essentials

public class TaxRate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Rate { get; set; }
}

#endregion

#region General / Organization

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
    public string TimeZone { get; set; }
}

public class Outlet
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public int CompanyId { get; set; }
}

public class Table
{
    public int Id { get; set; }
    public string Code { get; set; }
    public int OutletId { get; set; }
}

#endregion

#region Users & Roles

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class RoleClaim
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string ClaimType { get; set; }
    public string ClaimValue { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}

#endregion

#region Settings

public class UserSettings
{
    public int UserId { get; set; }
    public string Theme { get; set; }
    public string UiSettingsJson { get; set; }
}

#endregion

#region Sales Config / Channels

public class PriceGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class SalesChannel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsTakeout { get; set; }
}

#endregion

#region Modifiers

public class ModifierGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Modifier
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ModifierGroupId { get; set; }
}

#endregion

#region Product Items & Related

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
}

public class Allergen
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Barcode { get; set; }
    public int TaxRateId { get; set; }
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; }
}

public class ProductTag
{
    public int ProductId { get; set; }
    public int TagId { get; set; }
}

public class ProductAllergen
{
    public int ProductId { get; set; }
    public int AllergenId { get; set; }
}

public class ProductPrice
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int PriceGroupId { get; set; }
    public decimal Net { get; set; }
}

public class ProductModifierGroup
{
    public int ProductId { get; set; }
    public int ModifierGroupId { get; set; }
}

public class ProductList
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ProductListItem
{
    public int ProductListId { get; set; }
    public int ProductId { get; set; }
}

#endregion

#region Orders & Promotions

public class PromotionCode
{
    public int Id { get; set; }
    public string Code { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; }
    public decimal Discount { get; set; }
    public int LoyaltyPointsUsed { get; set; }
    public int? EventId { get; set; }
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int QuantityRealized { get; set; }
    public decimal NetPerOne { get; set; }
    public decimal VatPerOne { get; set; }
}

#endregion

#region Loyalty Program

public class LoyaltyPointLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Points { get; set; }
}

#endregion

#region Personal / Extras

public class Favorite
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
}

#endregion

#region Events

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public int? ProductListId { get; set; }
    public int? PriceGroupId { get; set; }
}

public class EventDay
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public DateTime Date { get; set; }
}

#endregion