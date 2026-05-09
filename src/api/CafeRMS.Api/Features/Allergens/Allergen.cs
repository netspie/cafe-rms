using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Allergens;

public class Allergen : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";

    private Allergen() { }

    public static Allergen Create(string name)
    {
        return new Allergen
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void Update(string name)
    {
        Name = name;
    }
}
