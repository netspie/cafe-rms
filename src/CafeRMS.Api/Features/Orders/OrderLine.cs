using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Orders;

public class OrderLine : IAuditable
{
    public Guid Id { get; private init; }
    public Guid OrderId { get; private init; }
    public Order? Order { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public int Quantity { get; private set; }
    public int QuantityRealized { get; private set; }
    public decimal NetPerOne { get; private set; }
    public decimal VatPerOne { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid? CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private OrderLine() { }

    public static OrderLine Create(Guid orderId, Guid productId, int quantity, decimal netPerOne, decimal vatPerOne,
        Guid? createdBy = null)
    {
        return new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            QuantityRealized = 0,
            NetPerOne = netPerOne,
            VatPerOne = vatPerOne,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
