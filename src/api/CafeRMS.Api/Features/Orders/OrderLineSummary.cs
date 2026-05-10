using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Features.Orders;

// Read model backed by the SQL view vw_order_lines_summary.
// One row per OrderLine with order header + product + customer denormalised in.
// Used by reports and ad-hoc admin queries that need a flat shape.
public sealed class OrderLineSummary
{
    public Guid LineId { get; init; }
    public int Quantity { get; init; }
    public decimal NetPerOne { get; init; }
    public decimal VatPerOne { get; init; }
    public decimal LineTotal { get; init; }

    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = "";

    public Guid OrderId { get; init; }
    public Guid OutletId { get; init; }
    public DateTimeOffset OrderCreatedAt { get; init; }
    public DateTimeOffset? OrderClosedAt { get; init; }
    public DateTimeOffset? OrderCancelledAt { get; init; }
    public string OrderStatus { get; init; } = "";

    public Guid? UserId { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
}
