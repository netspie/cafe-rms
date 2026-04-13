using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Features.SalesChannels;
using CafeRMS.Api.Features.Tables;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Orders;

public class Order : IAuditable, ISoftDeletable
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
    public OrderStatus Status { get; private set; }
    public decimal Discount { get; private set; }
    public int LoyaltyPointsUsed { get; private set; }
    public Guid? EventId { get; private init; }
    public Event? Event { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid? CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Order() { }

    public static Order Create(Guid outletId, Guid? tableId = null, Guid? salesChannelId = null,
        Guid? userId = null, Guid? eventId = null, decimal discount = 0, int loyaltyPointsUsed = 0,
        Guid? createdBy = null)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            OutletId = outletId,
            TableId = tableId,
            SalesChannelId = salesChannelId,
            UserId = userId,
            EventId = eventId,
            Status = OrderStatus.Open,
            Discount = discount,
            LoyaltyPointsUsed = loyaltyPointsUsed,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
