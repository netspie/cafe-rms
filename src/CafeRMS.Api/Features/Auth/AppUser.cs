using CafeRMS.Api.Features.Companies;
using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public AccountType AccountType { get; private init; }
    public Guid? CompanyId { get; private init; }
    public Company? Company { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }

    private AppUser() { }

    public static AppUser Create(string email, string firstName, string lastName, AccountType accountType, Guid? companyId = null)
    {
        return new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AccountType = accountType,
            CompanyId = companyId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
