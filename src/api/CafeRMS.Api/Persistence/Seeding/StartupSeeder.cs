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

        var hasSuperAdmin = await db.Users.AnyAsync(x => x.AccountType == AccountType.SuperAdmin);
        if (hasSuperAdmin)
            return;

        var user = AppUser.Create(email, "Super", "Admin", AccountType.SuperAdmin);
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed SuperAdmin: {Errors}", string.Join("; ", result.Errors.Select(x => x.Description)));
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

        // Idempotent — once the demo tenant exists, leave it alone.
        var alreadySeeded = await db.Companies.AnyAsync(x => x.LegalName == DemoCompanyLegalName);
        if (alreadySeeded)
            return;

        var input = new CompanyOwnerProvisioningInput(
            LegalName: DemoCompanyLegalName,
            TaxId: "0000000000",
            InvoicingAddress: "ul. Demo 1, 00-001 Warsaw",
            BillingEmail: "demo@cafe.local",
            BillingPhone: "+48000000000",
            IsPublic: false,
            OutletDisplayName: "Demo Cafe",
            OutletStreetAddress: "ul. Demo 1, 00-001 Warsaw",
            OutletPhone: "+48000000000",
            OutletTimeZone: "Europe/Warsaw",
            OutletCurrency: Currency.PLN,
            OutletLogoUrl: null,
            OwnerEmail: DemoOwnerEmail,
            OwnerPassword: ownerPassword,
            OwnerFirstName: "Demo",
            OwnerLastName: "Owner");

        await CompanyProvisioning.CreateCompanyWithOwnerAsync(input, userManager, roleManager, db);

        logger.LogInformation(
            "Seeded demo company {LegalName} with Owner staff {Email}",
            DemoCompanyLegalName, DemoOwnerEmail);
    }
}
