using System.Security.Claims;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Shared;

public static class ClaimsPrincipalExtensions
{
    public const string AccountTypeClaim = "accountType";
    public const string CompanyIdClaim = "companyId";
    public const string PermissionClaim = "permission";
    public const string CompanyIdSwitchHeader = "X-Company-Id";

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

        public AccountType? AccountType
        {
            get
            {
                var raw = user.FindFirstValue(AccountTypeClaim);
                return Enum.TryParse<AccountType>(raw, out var parsed) ? parsed : null;
            }
        }

        public Guid? CompanyId
        {
            get
            {
                var raw = user.FindFirstValue(CompanyIdClaim);
                return Guid.TryParse(raw, out var parsed) ? parsed : null;
            }
        }

        public bool HasPermission(string permission) =>
            user.HasClaim(PermissionClaim, permission);
    }
}
