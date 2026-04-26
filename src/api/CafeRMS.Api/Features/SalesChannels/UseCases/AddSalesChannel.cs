using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

public static class AddSalesChannel
{
    public sealed record Command(Guid CompanyId, string Name, bool IsTakeout);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a sales channel.");

        var nameTaken = await db.SalesChannels.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A sales channel named '{command.Name}' already exists.");

        var channel = SalesChannel.Create(command.Name, command.IsTakeout, command.CompanyId);
        db.SalesChannels.Add(channel);
        await db.SaveChangesAsync();
        return new Result(channel.Id);
    }
}
