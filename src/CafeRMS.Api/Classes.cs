// namespace CafeRMS.Api;
//
// public class Client
// {
//     public int Id { get; private init; }
//     public int UserId { get; private init; }
//     public required string Name { get; set; }
// }
//
// public enum Currency
// {
//     PLN,
//     EUR,
//     JPY
// }
//
// public class Outlet
// {
//     public int Id { get; private init; }
//     public Currency Currency { get; private init; }
//     public int ClientId { get; private init; }
// }
//
// public class Area
// {
//     public int Id { get; private init; }
//     public required string Name { get; set; }
//     public int OutletId { get; private init; }
// }
//
// public class Table
// {
//     public int Id { get; private init; }
//     public required string Name { get; set; }
//     public int AreaId { get; private init; }
// }
//
// public class SalesChannel
// {
//     public int Id { get; private init; }
//     public required string Name { get; set; }
// }
//
// public class PriceGroup
// {
//     public int Id { get; private init; }
//     public required string Name { get; set; }
// }
//
// public class Tag
// {
//     public int Id { get; private init; }
//     public required string Name { get; set; }
//     public required string ImageUrl { get; set; }
// }
//
// public class UnitOfMeasure
// {
//     public int Id { get; private init; }
//     public required string Name { get; set; }
//     public required string ShortName { get; set; }
// }
//
// public class Allergen
// {
//     public int Id { get; private init; }
//     public required string Name { get; set; }
// }
//
// public class SalesItem
// {
//     public int Id { get; private init; }
//     public string Name { get; set; } = "";
//     public int TaxRateId { get; private init; }
//     public int UnitOfMeasureId { get; private init; }
//     public ICollection<SalesItemTag> Tags { get; private set; } = [];
//     public ICollection<SalesItemPrice> Prices { get; private set; } = [];
//     public ICollection<SalesItemAllergen> Allergen { get; private set; } = [];
// }
//
// public class SalesItemTag
// {
//     public int Id { get; private init; }
//     public string Name { get; set; } = "";
//     public int ImageUrl { get; set; }
// }
//
// public class SalesItemPrice
// {
//     public int SalesItemId { get; private init; }
//     public int PriceGroupId { get; private init; }
//     public decimal Value { get; private set; }
// }
//
// public class SalesItemAllergen
// {
//     public int SalesItemId { get; private init; }
//     public int AllergenId { get; private init; }
// }
//
// public class Order
// {
//     public string Id { get; private init; }
//
//     public ICollection<OrderLine> OrderLines { get; private init; } = [];
// }
//
// public class OrderLine
// {
//     public string Id { get; private init; }
//     public int SalesItemId { get; private init; }
// }