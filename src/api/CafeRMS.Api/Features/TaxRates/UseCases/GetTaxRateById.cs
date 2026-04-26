using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.TaxRates.UseCases;

public static class GetTaxRateById
{
    public sealed record Result(Guid Id, string Name, string Description, decimal Rate, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var taxRate = await db.TaxRates
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.Description, x.Rate, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Tax rate not found.");

        return taxRate;
    }
}
