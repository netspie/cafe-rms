using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

public static class StartPreparingOrder
{
    public static async Task Execute(Guid id, AppDbContext db, DateTimeOffset now)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.IsCancelled || order.IsClosed)
            throw new ConflictException("Cannot start preparing an order that is already closed or cancelled.");
        if (order.AcceptedAt is null)
            throw new ConflictException("Order must be accepted before preparation can start.");
        if (order.InProgressAt is not null)
            throw new ConflictException("Preparation has already started.");

        order.StartPreparing(now);
        await db.SaveChangesAsync();
    }
}
