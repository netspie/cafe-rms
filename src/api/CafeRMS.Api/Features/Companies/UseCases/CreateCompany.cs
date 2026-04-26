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
        // Company fields
        string LegalName,
        string TaxId,
        string InvoicingAddress,
        string BillingEmail,
        string BillingPhone,
        bool IsPublic,
        // Outlet fields (1:1 with Company)
        string OutletDisplayName,
        string OutletStreetAddress,
        string OutletPhone,
        string OutletTimeZone,
        Currency OutletCurrency,
        string? OutletLogoUrl,
        // First Owner Staff user
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
        // Idempotency / uniqueness checks (app-level — no DB unique index on TaxId yet).
        var existingCompany = await db.Companies.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TaxId == command.TaxId);
        if (existingCompany is not null)
            throw new ConflictException($"A company with TaxId '{command.TaxId}' already exists.");

        var existingUser = await userManager.FindByEmailAsync(command.OwnerEmail);
        if (existingUser is not null)
            throw new ConflictException($"Email '{command.OwnerEmail}' is already in use.");

        await using var tx = await db.Database.BeginTransactionAsync();

        // Company
        var company = Company.Create(
            command.LegalName,
            command.TaxId,
            command.InvoicingAddress,
            command.BillingEmail,
            command.BillingPhone);
        company.IsPublic = command.IsPublic;
        db.Companies.Add(company);

        // Outlet (1:1 — unique index on Outlet.CompanyId enforces this)
        var outlet = Outlet.Create(
            command.OutletDisplayName,
            command.OutletStreetAddress,
            command.OutletPhone,
            command.OutletTimeZone,
            command.OutletCurrency,
            company.Id,
            command.OutletLogoUrl);
        db.Outlets.Add(outlet);

        await db.SaveChangesAsync();

        // Owner role for the new company
        var ownerRole = AppRole.Create(SystemRoles.Owner, company.Id);
        var roleResult = await roleManager.CreateAsync(ownerRole);
        if (!roleResult.Succeeded)
            throw new DomainException(string.Join("; ", roleResult.Errors.Select(e => e.Description)));

        // First Owner Staff user
        var ownerUser = AppUser.Create(
            command.OwnerEmail,
            command.OwnerFirstName,
            command.OwnerLastName,
            AccountType.Staff,
            company.Id);

        var createUserResult = await userManager.CreateAsync(ownerUser, command.OwnerPassword);
        if (!createUserResult.Succeeded)
            throw new DomainException(string.Join("; ", createUserResult.Errors.Select(e => e.Description)));

        // Assign Owner role directly via DbContext. UserManager.AddToRoleAsync would fail
        // because AppRole's ICompanyOwned global filter scopes by CurrentCompanyId, which
        // is the SuperAdmin's X-Company-Id header (or Guid.Empty if not set) — different
        // from the just-created company's id, so the new role would be invisible.
        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = ownerUser.Id, RoleId = ownerRole.Id });
        await db.SaveChangesAsync();

        await tx.CommitAsync();

        return new Result(company.Id, outlet.Id, ownerRole.Id, ownerUser.Id);
    }
}
