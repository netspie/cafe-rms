using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

public static class ListCompanies
{
    public sealed record Item(
        Guid Id,
        string LegalName,
        string TaxId,
        string BillingEmail,
        bool IsPublic,
        DateTimeOffset CreatedAt);

    public static async Task<IReadOnlyList<Item>> Execute(AppDbContext db) =>
        await db.Companies
            .OrderBy(c => c.LegalName)
            .Select(c => new Item(c.Id, c.LegalName, c.TaxId, c.BillingEmail, c.IsPublic, c.CreatedAt))
            .ToListAsync();
}
