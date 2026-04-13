using System.Security.Claims;

namespace CafeRMS.Api.Shared;

public static class ClaimsPrincipalExtensions
{
    public static Guid UserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found.");

        return Guid.Parse(id);
    }
}
