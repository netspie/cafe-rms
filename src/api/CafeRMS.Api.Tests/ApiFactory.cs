using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CafeRMS.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
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
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

                options.AddInterceptors(
                    sp.GetRequiredService<CafeRMS.Api.Persistence.Interceptors.AuditableSaveChangesInterceptor>(),
                    sp.GetRequiredService<CafeRMS.Api.Persistence.Interceptors.SoftDeletableSaveChangesInterceptor>());
            });
        });
    }

    public const string TestJwtIssuer = "CafeRMS";
    public const string TestJwtAudience = "CafeRMS";
    public const string TestJwtKey = "CHANGE-THIS-TO-A-LONG-SECRET-KEY-AT-LEAST-32-CHARS!";

    public HttpClient CreateAnonymousClient() => CreateClient();

    public async Task<AppUser> SeedUserAsync(
        AccountType accountType,
        string email,
        string password,
        string firstName = "Test",
        string lastName = "User")
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = AppUser.Create(email, firstName, lastName, accountType);
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException("SeedUserAsync failed: " + string.Join("; ", result.Errors.Select(x => x.Description)));
        return user;
    }

    public async Task<AppRole> SeedRoleAsync(string name, IEnumerable<string>? permissions = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var role = AppRole.Create(name);
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

    public async Task<Outlet> SeedOutletAsync(string displayName = "Test Outlet")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outlet = Outlet.Create(
            displayName, "ul. Test 1, 00-001 Warsaw", "+48000000000",
            "Europe/Warsaw", Currency.PLN);
        db.Outlets.Add(outlet);
        await db.SaveChangesAsync();
        return outlet;
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = roleId });
        await db.SaveChangesAsync();
    }

    public HttpClient CreateClientAs(
        AccountType accountType,
        Guid? userId = null,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? roles = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", BuildToken(accountType, userId, permissions, roles));
        return client;
    }

    private static string BuildToken(
        AccountType accountType,
        Guid? userId,
        IEnumerable<string>? permissions,
        IEnumerable<string>? roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimsPrincipalExtensions.AccountTypeClaim, accountType.ToString())
        };

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
