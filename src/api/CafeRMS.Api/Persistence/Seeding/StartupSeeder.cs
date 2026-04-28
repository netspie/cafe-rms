using CafeRMS.Api.Features.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Persistence.Seeding;

public class StartupSeeder(
    AppDbContext db,
    UserManager<AppUser> userManager,
    IConfiguration config,
    YumeyaDemoSeeder yumeyaDemoSeeder,
    ILogger<StartupSeeder> logger)
{
    public async Task SeedAsync()
    {
        await SeedSuperAdminAsync();
        await yumeyaDemoSeeder.SeedAsync();
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
}
