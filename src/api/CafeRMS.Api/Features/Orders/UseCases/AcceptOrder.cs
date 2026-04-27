using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

[ApiController]
public sealed class AcceptOrderController : ControllerBase
{
    [HttpPost("/api/orders/{id:guid}/accept")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await AcceptOrder.Execute(id, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}


public static class AcceptOrder
{
    public static async Task Execute(Guid id, AppDbContext db, DateTimeOffset now)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.IsCancelled || order.IsClosed)
            throw new ConflictException("Cannot accept an order that is already closed or cancelled.");
        if (order.AcceptedAt is not null)
            throw new ConflictException("Order is already accepted.");

        order.Accept(now);
        await db.SaveChangesAsync();
    }
}
