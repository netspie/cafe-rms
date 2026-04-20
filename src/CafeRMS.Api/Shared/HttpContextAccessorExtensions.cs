namespace CafeRMS.Api.Shared;

public static class HttpContextAccessorExtensions
{
    extension(IHttpContextAccessor accessor)
    {
        public Guid CurrentUserId
        {
            get
            {
                var httpContext = accessor.HttpContext
                    ?? throw new InvalidOperationException(
                        "No HTTP context is available on the current request.");

                return httpContext.User.UserId;
            }
        }
    }
}
