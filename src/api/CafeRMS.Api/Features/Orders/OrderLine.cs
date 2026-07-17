using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Orders;

public class OrderLine : Entity
{
    public Guid Id { get; private init; }
    public Guid OrderId { get; private init; }
    public Order? Order { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public int Quantity { get; private set; }
    public decimal NetPerOne { get; private set; }
    public decimal VatPerOne { get; private set; }
    public string? SelectedModifiers { get; private set; }

    private OrderLine() { }

    public static OrderLine Create(Guid orderId, Guid productId, int quantity, decimal netPerOne, decimal vatPerOne, string? selectedModifiers = null)
    {
        return new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            NetPerOne = netPerOne,
            VatPerOne = vatPerOne,
            SelectedModifiers = selectedModifiers
        };
    }
}
