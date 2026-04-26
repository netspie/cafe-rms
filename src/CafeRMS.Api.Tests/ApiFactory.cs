using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CafeRMS.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ASPNETCORE_ENVIRONMENT = Testing skips StartupSeeder and the auto-migrate dev-only path.
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Strip every EF Core registration the production setup added for Npgsql, then
            // wire AppDbContext to InMemory with its own internal service provider — sharing
            // one root provider across two DB providers (Npgsql + InMemory) trips EF's
            // "only a single database provider can be registered" guard.
            var efDescriptors = services.Where(x =>
                x.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                x.ServiceType == typeof(AppDbContext) ||
                x.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList();
            foreach (var d in efDescriptors)
                services.Remove(d);

            var inMemoryEfServices = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(dbName);
                options.UseInternalServiceProvider(inMemoryEfServices);

                // The InMemory provider doesn't support real transactions; downgrade the
                // warning so use cases calling BeginTransactionAsync don't blow up.
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

                // Wire up the same audit / soft-delete interceptors the production registration uses.
                options.AddInterceptors(
                    sp.GetRequiredService<CafeRMS.Api.Persistence.Interceptors.AuditableSaveChangesInterceptor>(),
                    sp.GetRequiredService<CafeRMS.Api.Persistence.Interceptors.SoftDeletableSaveChangesInterceptor>());
            });
        });
    }

    // Match the values committed in src/CafeRMS.Api/appsettings.json so the tokens we issue
    // here are accepted by the same JwtBearer setup the API uses at runtime.
    public const string TestJwtIssuer = "CafeRMS";
    public const string TestJwtAudience = "CafeRMS";
    public const string TestJwtKey = "CHANGE-THIS-TO-A-LONG-SECRET-KEY-AT-LEAST-32-CHARS!";

    public HttpClient CreateAnonymousClient() => CreateClient();

    public async Task<AppUser> SeedUserAsync(
        AccountType accountType,
        string email,
        string password,
        Guid? companyId = null,
        string firstName = "Test",
        string lastName = "User")
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = AppUser.Create(email, firstName, lastName, accountType, companyId);
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException("SeedUserAsync failed: " + string.Join("; ", result.Errors.Select(x => x.Description)));
        return user;
    }

    public async Task<AppRole> SeedRoleAsync(string name, Guid companyId, IEnumerable<string>? permissions = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var role = AppRole.Create(name, companyId);
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException("SeedRoleAsync failed: " + string.Join("; ", result.Errors.Select(x => x.Description)));
        foreach (var permission in permissions ?? [])
            db.RoleClaims.Add(new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = ClaimsPrincipalExtensions.PermissionClaim,
                ClaimValue = permission
            });
        await db.SaveChangesAsync();
        return role;
    }

    public HttpClient CreateClientAs(
        AccountType accountType,
        Guid? userId = null,
        Guid? companyId = null,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? roles = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", BuildToken(accountType, userId, companyId, permissions, roles));
        return client;
    }

    private static string BuildToken(
        AccountType accountType,
        Guid? userId,
        Guid? companyId,
        IEnumerable<string>? permissions,
        IEnumerable<string>? roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimsPrincipalExtensions.AccountTypeClaim, accountType.ToString())
        };

        if (companyId is Guid cid)
            claims.Add(new Claim(ClaimsPrincipalExtensions.CompanyIdClaim, cid.ToString()));

        foreach (var role in roles ?? [])
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions ?? [])
            claims.Add(new Claim(ClaimsPrincipalExtensions.PermissionClaim, permission));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: TestJwtIssuer,
            audience: TestJwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
