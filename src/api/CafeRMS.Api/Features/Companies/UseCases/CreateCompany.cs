using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Companies.UseCases;

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
