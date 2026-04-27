using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

[ApiController]
public sealed class MarkOrderReadyController : ControllerBase
{
    [HttpPost("/api/orders/{id:guid}/ready")]
    [Authorize(Policy = Permissions.OrdersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await MarkOrderReady.Execute(id, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}


public static class MarkOrderReady
{
    public static async Task Execute(Guid id, AppDbContext db, DateTimeOffset now)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.IsCancelled || order.IsClosed)
            throw new ConflictException("Cannot mark an order ready that is already closed or cancelled.");
        if (order.InProgressAt is null)
            throw new ConflictException("Order must be in progress before it can be marked ready.");
        if (order.ReadyAt is not null)
            throw new ConflictException("Order is already ready.");

        order.MarkReady(now);
        await db.SaveChangesAsync();
    }
}
