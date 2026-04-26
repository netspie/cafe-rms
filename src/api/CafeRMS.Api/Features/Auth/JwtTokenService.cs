using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CafeRMS.Api.Features.Auth;

public sealed record GeneratedToken(string AccessToken, DateTimeOffset ExpiresAt);

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptions, AppDbContext db)
{
    private readonly JwtOptions options = jwtOptions.Value;

    public async Task<GeneratedToken> GenerateAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimsPrincipalExtensions.AccountTypeClaim, user.AccountType.ToString())
        };

        if (user.CompanyId is Guid companyId)
            claims.Add(new Claim(ClaimsPrincipalExtensions.CompanyIdClaim, companyId.ToString()));

        // Login runs without a company context: the AppRole ICompanyOwned filter would
        // hide the user's roles (the JWT being generated IS what carries the companyId),
        // so bypass filters and join roles by user id directly.
        var userRoles = await db.UserRoles
            .IgnoreQueryFilters()
            .Where(x => x.UserId == user.Id)
            .Join(db.Roles.IgnoreQueryFilters(), x => x.RoleId, x => x.Id, (x, y) => y)
            .Where(x => x.DeletedAt == null)
            .ToListAsync();

        foreach (var role in userRoles)
        {
            var roleName = role.Name ?? "";
            if (roleName.Length == 0)
                continue;

            claims.Add(new Claim(ClaimTypes.Role, roleName));

            // Owner bypasses permission checks at the policy handler — no permission claims needed.
            if (string.Equals(roleName, SystemRoles.Owner, StringComparison.Ordinal))
                continue;

            var permissionClaims = await db.RoleClaims
                .Where(x => x.RoleId == role.Id && x.ClaimType == ClaimsPrincipalExtensions.PermissionClaim)
                .Select(x => x.ClaimValue)
                .ToListAsync();

            foreach (var permission in permissionClaims)
                if (!string.IsNullOrEmpty(permission))
                    claims.Add(new Claim(ClaimsPrincipalExtensions.PermissionClaim, permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(options.ExpiryHours);
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new GeneratedToken(jwt, expiresAt);
    }
}
