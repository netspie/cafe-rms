using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

public static class ListPublicCompanies
{
    public sealed record Item(
        Guid Id,
        string LegalName,
        string DisplayName,
        string StreetAddress,
        string Phone,
        string TimeZone,
        string Currency,
        string? LogoUrl);

    public static async Task<IReadOnlyList<Item>> Execute(AppDbContext db) =>
        await db.Outlets
            .Where(x => x.Company!.IsPublic)
            .Select(x => new Item(
                x.CompanyId,
                x.Company!.LegalName,
                x.DisplayName,
                x.StreetAddress,
                x.Phone,
                x.TimeZone,
                x.Currency.ToString(),
                x.LogoUrl))
            .ToListAsync();
}
