using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth;

public class AppRole : IdentityRole<Guid>, ICompanyOwned, IAuditable, ISoftDeletable
{
    public Guid CompanyId { get; private init; }
    public Company? Company { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private AppRole() { }

    public static AppRole Create(string name, Guid companyId)
    {
        return new AppRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            CompanyId = companyId
        };
    }
}
