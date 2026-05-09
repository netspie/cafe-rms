using CafeRMS.Api.Shared.Entities;
using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth;

public class AppUser : IdentityUser<Guid>, IAuditable, ISoftDeletable
{
    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public AccountType AccountType { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private AppUser() { }

    public static AppUser Create(string email, string firstName, string lastName, AccountType accountType)
    {
        return new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AccountType = accountType
        };
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}
