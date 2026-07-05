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
    public async Task<GetOrderById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");
        if (order.UserId != User.UserId)
            throw new NotFoundException("Order not found.");

        return await GetOrderById.Execute(id, db);
    }
}

public static class GetOrderById
{
    public sealed record Result(
        Guid Id,
        Guid OutletId,
        Guid? TableId,
        string? TableName,
        Guid? SalesChannelId,
        Guid? UserId,
        Guid? EventId,
        Guid? PromotionCodeId,
        decimal Discount,
        int LoyaltyPointsUsed,
        OrderStatus Status,
        DateTimeOffset? ClosedAt,
        DateTimeOffset? CancelledAt,
        string? CancellationReason,
        IReadOnlyList<LineInfo> Lines,
        DateTimeOffset CreatedAt);

    public sealed record LineInfo(Guid Id, Guid ProductId, string ProductName, int Quantity, decimal NetPerOne, decimal VatPerOne);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        var tableName = order.TableId == null
            ? null
            : await db.Tables.Where(t => t.Id == order.TableId).Select(t => t.Name).FirstOrDefaultAsync();

        var lines = await db.OrderLines
            .Where(x => x.OrderId == id)
            .Join(
                db.Products,
                ol => ol.ProductId,
                p => p.Id,
                (ol, p) => new LineInfo(ol.Id, ol.ProductId, p.Name, ol.Quantity, ol.NetPerOne, ol.VatPerOne))
            .ToListAsync();

        return new Result(
            order.Id, order.OutletId, order.TableId, tableName, order.SalesChannelId, order.UserId, order.EventId,
            order.PromotionCodeId, order.Discount, order.LoyaltyPointsUsed,
            order.Status, order.ClosedAt, order.CancelledAt, order.CancellationReason,
            lines, order.CreatedAt);
    }
}
