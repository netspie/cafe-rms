using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

[ApiController]
public sealed class GetOrderByIdController : ControllerBase
{
    [HttpGet("/api/orders/{id:guid}")]
    [Authorize(Policy = Permissions.OrdersView)]
    public async Task<GetOrderById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetOrderById.Execute(id, db);
}

[ApiController]
public sealed class GetMyOrderByIdController : ControllerBase
{
    [HttpGet("/api/my/orders/{id:guid}")]
    [Authorize(Policy = Policies.RequireGuest)]
    [ResourceOwner<Order>("id", nameof(Order.UserId))]
    public async Task<GetOrderById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetOrderById.Execute(id, db);
}


public static class GetOrderById
{
    public sealed record Result(
        Guid Id,
        Guid OutletId,
        Guid? TableId,
        Guid? SalesChannelId,
        Guid? UserId,
        Guid? EventId,
        Guid? PromotionCodeId,
        decimal Discount,
        int LoyaltyPointsUsed,
        OrderStatus Status,
        DateTimeOffset? AcceptedAt,
        DateTimeOffset? InProgressAt,
        DateTimeOffset? ReadyAt,
        DateTimeOffset? ClosedAt,
        DateTimeOffset? CancelledAt,
        string? CancellationReason,
        IReadOnlyList<LineInfo> Lines,
        DateTimeOffset CreatedAt);

    public sealed record LineInfo(Guid Id, Guid ProductId, int Quantity, decimal NetPerOne, decimal VatPerOne);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        var lines = await db.OrderLines
            .Where(x => x.OrderId == id)
            .Select(x => new LineInfo(x.Id, x.ProductId, x.Quantity, x.NetPerOne, x.VatPerOne))
            .ToListAsync();

        return new Result(
            order.Id, order.OutletId, order.TableId, order.SalesChannelId, order.UserId, order.EventId,
            order.PromotionCodeId, order.Discount, order.LoyaltyPointsUsed,
            order.Status, order.AcceptedAt, order.InProgressAt, order.ReadyAt,
            order.ClosedAt, order.CancelledAt, order.CancellationReason,
            lines, order.CreatedAt);
    }
}
