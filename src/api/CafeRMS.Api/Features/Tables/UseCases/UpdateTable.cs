using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Tables.UseCases;

public static class UpdateTable
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var table = await db.Tables.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Table not found.");

        var nameTaken = await db.Tables.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A table named '{command.Name}' already exists.");

        table.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
