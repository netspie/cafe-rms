using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tables.UseCases;

public static class AddTable
{
    public sealed record Command(Guid CompanyId, string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a table.");

        // OutletId is resolved server-side: every company has exactly one Outlet (1:1 enforced
        // by unique index on Outlet.CompanyId), so the caller doesn't pass it.
        var outletId = await db.Outlets
            .Where(x => x.CompanyId == command.CompanyId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Outlet not found for the current company.");

        var nameTaken = await db.Tables.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A table named '{command.Name}' already exists.");

        var table = Table.Create(command.Name, outletId, command.CompanyId);
        db.Tables.Add(table);
        await db.SaveChangesAsync();
        return new Result(table.Id);
    }
}
