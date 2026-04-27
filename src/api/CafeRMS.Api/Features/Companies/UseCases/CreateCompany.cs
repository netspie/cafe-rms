using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

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


public static class CreateCompany
{
    public sealed record Command(
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

    public sealed record Result(
        Guid CompanyId,
        Guid OutletId,
        Guid OwnerRoleId,
        Guid OwnerUserId);

    public static async Task<Result> Execute(
        Command command,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        AppDbContext db)
    {
        // App-level uniqueness checks — surface clean 409s before the provisioning transaction.
        // (Race-condition window past these checks is also caught by the unique index on TaxId.)
        var taxIdTaken = await db.Companies.IgnoreQueryFilters()
            .AnyAsync(x => x.TaxId == command.TaxId);
        if (taxIdTaken)
            throw new ConflictException($"A company with TaxId '{command.TaxId}' already exists.");

        var emailTaken = await userManager.FindByEmailAsync(command.OwnerEmail) is not null;
        if (emailTaken)
            throw new ConflictException($"Email '{command.OwnerEmail}' is already in use.");

        var input = new CompanyOwnerProvisioningInput(
            command.LegalName, command.TaxId, command.InvoicingAddress,
            command.BillingEmail, command.BillingPhone, command.IsPublic,
            command.OutletDisplayName, command.OutletStreetAddress, command.OutletPhone,
            command.OutletTimeZone, command.OutletCurrency, command.OutletLogoUrl,
            command.OwnerEmail, command.OwnerPassword, command.OwnerFirstName, command.OwnerLastName);

        var result = await CompanyProvisioning.CreateCompanyWithOwnerAsync(input, userManager, roleManager, db);

        return new Result(result.CompanyId, result.OutletId, result.OwnerRoleId, result.OwnerUserId);
    }
}
