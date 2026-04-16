using System.Security.Claims;

namespace CafeRMS.Api.Shared;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public Guid UserId
        {
            get
            {
                var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("User ID claim not found.");

                return Guid.Parse(id);
            }
        }
    }
}
