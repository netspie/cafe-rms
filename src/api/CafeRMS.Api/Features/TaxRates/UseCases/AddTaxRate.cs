using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

public static class AddTaxRate
{
    public sealed record Command(Guid CompanyId, string Name, string Description, decimal Rate);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a tax rate.");

        var nameTaken = await db.TaxRates.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"A tax rate named '{command.Name}' already exists.");

        var taxRate = TaxRate.Create(command.Name, command.Description, command.Rate, command.CompanyId);
        db.TaxRates.Add(taxRate);
        await db.SaveChangesAsync();
        return new Result(taxRate.Id);
    }
}
