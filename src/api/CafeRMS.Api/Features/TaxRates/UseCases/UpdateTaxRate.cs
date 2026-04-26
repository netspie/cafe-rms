using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

public static class UpdateTaxRate
{
    public sealed record Command(Guid Id, string Name, string Description, decimal Rate);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var taxRate = await db.TaxRates.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Tax rate not found.");

        var nameTaken = await db.TaxRates.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A tax rate named '{command.Name}' already exists.");

        taxRate.Update(command.Name, command.Description, command.Rate);
        await db.SaveChangesAsync();
    }
}
