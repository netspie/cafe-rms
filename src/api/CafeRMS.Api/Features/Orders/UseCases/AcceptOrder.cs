using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

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
