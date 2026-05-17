using System.Security.Claims;

namespace CafeRMS.Api.Shared;

public static class HttpContextAccessorExtensions
{
    extension(IHttpContextAccessor accessor)
    {
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
