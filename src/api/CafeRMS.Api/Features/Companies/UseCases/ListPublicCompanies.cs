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

    // Anonymous discovery: caller's CurrentCompanyId is Guid.Empty, which would make the
    // Outlet ICompanyOwned filter exclude every row. Bypass filters and re-add the soft-delete
    // checks explicitly. Public listing is gated by Company.IsPublic, not the per-tenant filter.
    public static async Task<IReadOnlyList<Item>> Execute(AppDbContext db) =>
        await db.Outlets
            .IgnoreQueryFilters()
            .Where(x => x.DeletedAt == null && x.Company!.DeletedAt == null && x.Company!.IsPublic)
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
