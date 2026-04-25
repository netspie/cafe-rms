using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Features.Outlets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Persistence.Seeding;

public class StartupSeeder(
    AppDbContext db,
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IConfiguration config,
    ILogger<StartupSeeder> logger)
{
    private const string DemoCompanyLegalName = "Demo Cafe Sp. z o.o.";
    private const string DemoOwnerEmail = "owner@demo.cafe";

    public async Task SeedAsync()
    {
        await SeedSuperAdminAsync();
        await SeedDemoCompanyAsync();
    }

    private async Task SeedSuperAdminAsync()
    {
        var email = config["Seed:SuperAdminEmail"];
        var password = config["Seed:SuperAdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:SuperAdminEmail / Seed:SuperAdminPassword not set; skipping SuperAdmin seed.");
            return;
        }

        var hasSuperAdmin = await db.Users.AnyAsync(u => u.AccountType == AccountType.SuperAdmin);
        if (hasSuperAdmin)
            return;

        var user = AppUser.Create(email, "Super", "Admin", AccountType.SuperAdmin);
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed SuperAdmin: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogInformation("Seeded SuperAdmin {Email}", email);
    }

    private async Task SeedDemoCompanyAsync()
    {
        var ownerPassword = config["Seed:DemoOwnerPassword"];
        if (string.IsNullOrWhiteSpace(ownerPassword))
        {
            logger.LogWarning("Seed:DemoOwnerPassword not set; skipping demo company + Owner seed.");
            return;
        }

        var company = await db.Companies.FirstOrDefaultAsync(c => c.LegalName == DemoCompanyLegalName);
        if (company is null)
        {
            company = Company.Create(DemoCompanyLegalName, "0000000000", "ul. Demo 1, 00-001 Warsaw", "demo@cafe.local", "+48000000000");
            db.Companies.Add(company);
            db.Outlets.Add(Outlet.Create("Demo Cafe", "ul. Demo 1, 00-001 Warsaw", "+48000000000", "Europe/Warsaw", Currency.PLN, company.Id));
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded demo company {LegalName}", DemoCompanyLegalName);
        }

        // AppRole has an ICompanyOwned global filter; in seeder context CurrentCompanyId
        // is Guid.Empty, so the filter would hide every role. Bypass it for the existence check.
        var ownerRole = await db.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.CompanyId == company.Id && r.NormalizedName == SystemRoles.Owner.ToUpperInvariant());

        if (ownerRole is null)
        {
            ownerRole = AppRole.Create(SystemRoles.Owner, company.Id);
            var roleResult = await roleManager.CreateAsync(ownerRole);
            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to seed Owner role: {Errors}", string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                return;
            }
            logger.LogInformation("Seeded Owner role for {LegalName}", DemoCompanyLegalName);
        }

        var owner = await userManager.FindByEmailAsync(DemoOwnerEmail);
        if (owner is null)
        {
            owner = AppUser.Create(DemoOwnerEmail, "Demo", "Owner", AccountType.Staff, company.Id);
            var ownerResult = await userManager.CreateAsync(owner, ownerPassword);
            if (!ownerResult.Succeeded)
            {
                logger.LogError("Failed to seed demo Owner: {Errors}", string.Join("; ", ownerResult.Errors.Select(e => e.Description)));
                return;
            }
            logger.LogInformation("Seeded demo Owner staff {Email}", DemoOwnerEmail);
        }

        var alreadyAssigned = await db.UserRoles.AnyAsync(ur => ur.UserId == owner.Id && ur.RoleId == ownerRole.Id);
        if (!alreadyAssigned)
        {
            db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = owner.Id, RoleId = ownerRole.Id });
            await db.SaveChangesAsync();
        }
    }
}
