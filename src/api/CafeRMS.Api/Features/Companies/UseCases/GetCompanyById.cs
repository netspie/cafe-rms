using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

[ApiController]
public sealed class GetCompanyByIdController : ControllerBase
{
    [HttpGet("/api/companies/{id:guid}")]
    public async Task<GetCompanyById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        // SuperAdmin sees any; Staff sees only their own; Guest never sees a company by id.
        var isSuperAdmin = User.AccountType == AccountType.SuperAdmin;
        var ownsThisCompany = User.CompanyId == id;
        if (!isSuperAdmin && !ownsThisCompany)
            throw new ForbiddenException("You do not have access to this company.");

        return await GetCompanyById.Execute(id, db);
    }
}


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

        // Outlet has the ICompanyOwned global filter; bypass it here so SuperAdmin (whose
        // CurrentCompanyId resolves to Guid.Empty without an X-Company-Id switch) can still
        // load the outlet for any company they're inspecting. The DeletedAt check is kept
        // explicit since IgnoreQueryFilters drops the soft-delete filter too.
        var outlet = await db.Outlets.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.CompanyId == id && x.DeletedAt == null)
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
