using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

public static class GetCompanyById
{
    public sealed record Result(
        Guid Id,
        string LegalName,
        string TaxId,
        string InvoicingAddress,
        string BillingEmail,
        string BillingPhone,
        bool IsPublic,
        DateTimeOffset CreatedAt,
        OutletInfo Outlet);

    public sealed record OutletInfo(
        Guid Id,
        string DisplayName,
        string StreetAddress,
        string Phone,
        string TimeZone,
        string Currency,
        string? LogoUrl);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var company = await db.Companies.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Company not found.");

        var outlet = await db.Outlets.FirstOrDefaultAsync(x => x.CompanyId == id)
            ?? throw new NotFoundException("Outlet not found for this company.");

        return new Result(
            company.Id,
            company.LegalName,
            company.TaxId,
            company.InvoicingAddress,
            company.BillingEmail,
            company.BillingPhone,
            company.IsPublic,
            company.CreatedAt,
            new OutletInfo(
                outlet.Id,
                outlet.DisplayName,
                outlet.StreetAddress,
                outlet.Phone,
                outlet.TimeZone,
                outlet.Currency.ToString(),
                outlet.LogoUrl));
    }
}
