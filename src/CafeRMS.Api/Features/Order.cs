namespace CafeRMS.Api.Features;

public class PromotionCode
{
    public int Id { get; private init; }
    public string Code { get; init; } = "";
    public decimal DiscountPercentage { get; set; }
}

public class Order
{
    public int Id { get; private init; }
    public int? UserId { get; set; }
    public string Name { get; init; } = "";
    public decimal Discount { get; set; }
    public int LoyaltyPointsUsed { get; set; }
    public int? EventId { get; set; }
}

public class OrderLine
{
    public int Id { get; private init; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int QuantityRealized { get; set; }
    public decimal NetPerOne { get; set; }
    public decimal VatPerOne { get; set; }
}
