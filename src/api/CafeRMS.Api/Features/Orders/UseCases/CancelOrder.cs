using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

public static class CancelOrder
{
    // calledByCustomer = true → only allowed before AcceptedAt (customer can't take back
    // an order kitchen has already accepted).
    public static async Task Execute(Guid id, string? reason, bool calledByCustomer, AppDbContext db, DateTimeOffset now)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.IsCancelled)
            throw new ConflictException("Order is already cancelled.");
        if (order.IsClosed)
            throw new ConflictException("Cannot cancel a closed order.");
        if (calledByCustomer && order.AcceptedAt is not null)
            throw new ForbiddenException("Order can no longer be cancelled by the customer — it has been accepted.");

        order.Cancel(now, reason);
        await db.SaveChangesAsync();
    }
}
