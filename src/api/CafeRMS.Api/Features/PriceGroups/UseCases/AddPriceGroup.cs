using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PriceGroups.UseCases;

public static class AddPriceGroup
{
    public sealed record Command(Guid CompanyId, string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a price group.");

        var nameTaken = await db.PriceGroups.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A price group named '{command.Name}' already exists.");

        var priceGroup = PriceGroup.Create(command.Name, command.CompanyId);
        db.PriceGroups.Add(priceGroup);
        await db.SaveChangesAsync();
        return new Result(priceGroup.Id);
    }
}
