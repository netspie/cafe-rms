using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CafeRMS.Api.Features.Auth;

public sealed record GeneratedToken(string AccessToken, DateTimeOffset ExpiresAt);

public sealed class JwtTokenService(
    IOptions<JwtOptions> jwtOptions,
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager)
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

        var roles = await userManager.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            // Owner bypasses permission checks at the policy handler — no permission claims needed.
            if (string.Equals(roleName, SystemRoles.Owner, StringComparison.Ordinal))
                continue;

            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
                continue;

            var roleClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
                if (claim.Type == ClaimsPrincipalExtensions.PermissionClaim)
                    claims.Add(new Claim(ClaimsPrincipalExtensions.PermissionClaim, claim.Value));
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
