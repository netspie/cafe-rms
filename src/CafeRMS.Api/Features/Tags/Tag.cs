using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Tags;

public class Tag : IAuditable, ISoftDeletable, ICompanyScoped
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? ImageUrl { get; private set; }
    public Guid CompanyId { get; private init; }
    public Company? Company { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Tag() { }

    public static Tag Create(string name, Guid companyId, string? imageUrl = null)
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            ImageUrl = imageUrl,
            CompanyId = companyId
        };
    }
}
