using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies.UseCases;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Companies.Controllers;

[ApiController]
public sealed class CreateCompanyController : ControllerBase
{
    [HttpPost("/api/companies")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<CreateCompanyResponse> Handle(
        [FromBody] CreateCompanyRequest request,
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] RoleManager<AppRole> roleManager,
        [FromServices] AppDbContext db)
    {
        var command = new CreateCompany.Command(
            request.LegalName,
            request.TaxId,
            request.InvoicingAddress,
            request.BillingEmail,
            request.BillingPhone,
            request.IsPublic,
            request.OutletDisplayName,
            request.OutletStreetAddress,
            request.OutletPhone,
            request.OutletTimeZone,
            request.OutletCurrency,
            request.OutletLogoUrl,
            request.OwnerEmail,
            request.OwnerPassword,
            request.OwnerFirstName,
            request.OwnerLastName);

        var result = await CreateCompany.Execute(command, userManager, roleManager, db);

        return new CreateCompanyResponse(result.CompanyId, result.OutletId, result.OwnerRoleId, result.OwnerUserId);
    }
}

public sealed record CreateCompanyRequest(
    string LegalName,
    string TaxId,
    string InvoicingAddress,
    string BillingEmail,
    string BillingPhone,
    bool IsPublic,
    string OutletDisplayName,
    string OutletStreetAddress,
    string OutletPhone,
    string OutletTimeZone,
    Currency OutletCurrency,
    string? OutletLogoUrl,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerFirstName,
    string OwnerLastName);

public sealed record CreateCompanyResponse(
    Guid CompanyId,
    Guid OutletId,
    Guid OwnerRoleId,
    Guid OwnerUserId);

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyValidator()
    {
        // Company
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InvoicingAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BillingEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.BillingPhone).NotEmpty().MaximumLength(50);

        // Outlet
        RuleFor(x => x.OutletDisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OutletStreetAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OutletPhone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OutletTimeZone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OutletLogoUrl).MaximumLength(500);

        // Owner staff
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.OwnerPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.OwnerFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OwnerLastName).NotEmpty().MaximumLength(100);
    }
}
