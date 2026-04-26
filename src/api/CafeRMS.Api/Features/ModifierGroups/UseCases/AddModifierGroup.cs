using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

public static class AddModifierGroup
{
    public sealed record Command(Guid CompanyId, string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a modifier group.");

        var nameTaken = await db.ModifierGroups.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A modifier group named '{command.Name}' already exists.");

        var group = ModifierGroup.Create(command.Name, command.CompanyId);
        db.ModifierGroups.Add(group);
        await db.SaveChangesAsync();
        return new Result(group.Id);
    }
}
