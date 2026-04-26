using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Companies;

public sealed record CompanyOwnerProvisioningInput(
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

public sealed record CompanyOwnerProvisioningResult(
    Guid CompanyId,
    Guid OutletId,
    Guid OwnerRoleId,
    Guid OwnerUserId);

// Shared bootstrap: creates Company + Outlet (1:1) + Owner role + first Owner Staff user
// and links them in one transaction. Used by:
//   * Features/Companies/UseCases/CreateCompany — POST /api/companies (SuperAdmin)
//   * Persistence/Seeding/StartupSeeder        — demo tenant on dev/demo runs
//
// Each caller handles its own pre-flight checks (uniqueness rejection vs idempotency skip)
// — this method assumes inputs are clean and never returns "already exists"; it always
// creates fresh rows or rolls everything back on failure.
//
// Owner role-assignment uses db.UserRoles.Add directly because AppRole's ICompanyOwned
// global filter scopes by CurrentCompanyId, which is Guid.Empty in the seeder and the
// SuperAdmin's X-Company-Id header (a different company) in POST /api/companies — either
// way the just-created role would be invisible to UserManager.AddToRoleAsync.
public static class CompanyProvisioning
{
    public static async Task<CompanyOwnerProvisioningResult> CreateCompanyWithOwnerAsync(
        CompanyOwnerProvisioningInput input,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        AppDbContext db)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var company = Company.Create(
            input.LegalName,
            input.TaxId,
            input.InvoicingAddress,
            input.BillingEmail,
            input.BillingPhone);
        company.IsPublic = input.IsPublic;
        db.Companies.Add(company);

        var outlet = Outlet.Create(
            input.OutletDisplayName,
            input.OutletStreetAddress,
            input.OutletPhone,
            input.OutletTimeZone,
            input.OutletCurrency,
            company.Id,
            input.OutletLogoUrl);
        db.Outlets.Add(outlet);

        await db.SaveChangesAsync();

        var ownerRole = AppRole.Create(SystemRoles.Owner, company.Id);
        var roleResult = await roleManager.CreateAsync(ownerRole);
        if (!roleResult.Succeeded)
            throw new DomainException(string.Join("; ", roleResult.Errors.Select(x => x.Description)));

        var ownerUser = AppUser.Create(
            input.OwnerEmail,
            input.OwnerFirstName,
            input.OwnerLastName,
            AccountType.Staff,
            company.Id);

        var createUserResult = await userManager.CreateAsync(ownerUser, input.OwnerPassword);
        if (!createUserResult.Succeeded)
            throw new DomainException(string.Join("; ", createUserResult.Errors.Select(x => x.Description)));

        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = ownerUser.Id, RoleId = ownerRole.Id });
        await db.SaveChangesAsync();

        await tx.CommitAsync();

        return new CompanyOwnerProvisioningResult(company.Id, outlet.Id, ownerRole.Id, ownerUser.Id);
    }
}
