using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Tags;

public class Tag : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? ImageUrl { get; private set; }

    private Tag() { }

    public static Tag Create(string name, string? imageUrl = null)
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            ImageUrl = imageUrl
        };
    }

    public void Update(string name, string? imageUrl)
    {
        Name = name;
        ImageUrl = imageUrl;
    }
}
