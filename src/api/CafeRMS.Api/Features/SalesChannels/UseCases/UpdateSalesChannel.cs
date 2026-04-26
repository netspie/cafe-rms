using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

public static class UpdateSalesChannel
{
    public sealed record Command(Guid Id, string Name, bool IsTakeout);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var channel = await db.SalesChannels.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Sales channel not found.");

        var nameTaken = await db.SalesChannels.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A sales channel named '{command.Name}' already exists.");

        channel.Update(command.Name, command.IsTakeout);
        await db.SaveChangesAsync();
    }
}
