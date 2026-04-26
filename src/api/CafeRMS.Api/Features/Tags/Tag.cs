using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Tags;

public class Tag : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? ImageUrl { get; private set; }

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
