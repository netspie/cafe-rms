using System.Security.Claims;

namespace CafeRMS.Api.Shared;

public static class HttpContextAccessorExtensions
{
    extension(IHttpContextAccessor accessor)
    {
        // Returns the current user's id from the JWT 'sub' claim, or Guid.Empty when there's
        // no HTTP context (StartupSeeder, background work) or no user claim (anonymous flows
        // like /auth/register/guest). Used by the Auditable / SoftDeletable interceptors,
        // where Guid.Empty correctly represents "no human actor — system action".
        public Guid CurrentUserId
        {
            get
            {
                var httpContext = accessor.HttpContext;
                if (httpContext is null)
                    return Guid.Empty;
                var raw = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
            }
        }
    }
}
