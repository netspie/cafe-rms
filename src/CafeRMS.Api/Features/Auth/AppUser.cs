using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private init; }

    private AppUser() { }

    public static AppUser Create(string email, string firstName, string lastName)
    {
        return new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
