using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth;

public class AppRole : IdentityRole<Guid>
{
    private AppRole() { }

    public static AppRole Create(string name)
    {
        return new AppRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant()
        };
    }
}
