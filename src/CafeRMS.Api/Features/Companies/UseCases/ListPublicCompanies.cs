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
            .Where(o => o.Company!.IsPublic)
            .Select(o => new Item(
                o.CompanyId,
                o.Company!.LegalName,
                o.DisplayName,
                o.StreetAddress,
                o.Phone,
                o.TimeZone,
                o.Currency.ToString(),
                o.LogoUrl))
            .ToListAsync();
}
