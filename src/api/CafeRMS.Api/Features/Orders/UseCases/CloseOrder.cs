using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

[ApiController]
public sealed class CloseOrderController : ControllerBase
{
    [HttpPost("/api/orders/{id:guid}/close")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await CloseOrder.Execute(id, db.CurrentCompanyId, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}


public static class CloseOrder
{
    public static async Task Execute(Guid id, Guid companyId, AppDbContext db, DateTimeOffset now)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.IsCancelled)
            throw new ConflictException("Cannot close a cancelled order.");
        if (order.IsClosed)
            throw new ConflictException("Order is already closed.");

        await using var tx = await db.Database.BeginTransactionAsync();

        order.Close(now);

        // Loyalty earn: 1 point per integer unit of currency net (after discount).
        // Walk-ins (no UserId) don't earn anything. No-op if there are no points to earn.
        if (order.UserId is Guid userId)
        {
            var lineNetTotal = await db.OrderLines
                .Where(x => x.OrderId == id)
                .SumAsync(x => (decimal?)(x.NetPerOne * x.Quantity)) ?? 0m;
            var earnedPoints = (int)Math.Floor(lineNetTotal - order.Discount);
            if (earnedPoints > 0)
            {
                db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(
                    userId,
                    earnedPoints,
                    companyId,
                    reason: $"Earned on order {order.Id}"));
            }
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
