using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Features.PromotionCodes;
using CafeRMS.Api.Features.SalesChannels;
using CafeRMS.Api.Features.Tables;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Orders;

public class Order : Entity
{
    public Guid Id { get; private init; }
    public Guid OutletId { get; private init; }
    public Outlet? Outlet { get; private init; }
    public Guid? TableId { get; private init; }
    public Table? Table { get; private init; }
    public Guid? SalesChannelId { get; private init; }
    public SalesChannel? SalesChannel { get; private init; }
    public Guid? UserId { get; private init; }
    public AppUser? User { get; private init; }
    public Guid? PromotionCodeId { get; private set; }
    public PromotionCode? PromotionCode { get; private init; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public decimal Discount { get; private set; }
    public int LoyaltyPointsUsed { get; private set; }
    public Guid? EventId { get; private init; }
    public Event? Event { get; private init; }

    public bool IsClosed => ClosedAt is not null;
    public bool IsCancelled => CancelledAt is not null;

    public OrderStatus Status =>
        IsCancelled ? OrderStatus.Cancelled
        : IsClosed ? OrderStatus.Closed
        : OrderStatus.Placed;

    private Order() { }

    public static Order Create(
        Guid outletId,
        Guid? tableId = null,
        Guid? salesChannelId = null,
        Guid? userId = null,
        Guid? eventId = null,
        Guid? promotionCodeId = null,
        decimal discount = 0,
        int loyaltyPointsUsed = 0)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            OutletId = outletId,
            TableId = tableId,
            SalesChannelId = salesChannelId,
            UserId = userId,
            EventId = eventId,
            PromotionCodeId = promotionCodeId,
            Discount = discount,
            LoyaltyPointsUsed = loyaltyPointsUsed
        };
    }

    public void Close(DateTimeOffset now) => ClosedAt = now;

    public void Cancel(DateTimeOffset now, string? reason)
    {
        CancelledAt = now;
        CancellationReason = reason;
    }

    public void AssignPromotion(Guid promotionCodeId, decimal discount)
    {
        PromotionCodeId = promotionCodeId;
        Discount = discount;
    }
}
