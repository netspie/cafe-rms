using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

public static class DeleteTaxRate
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var taxRate = await db.TaxRates.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Tax rate not found.");

        db.TaxRates.Remove(taxRate);
        await db.SaveChangesAsync();
    }
}
