using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Modifiers;

public class Modifier : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public Guid ModifierGroupId { get; private init; }
    public ModifierGroup? ModifierGroup { get; private init; }

    private Modifier() { }

    public static Modifier Create(string name, Guid modifierGroupId)
    {
        return new Modifier
        {
            Id = Guid.NewGuid(),
            Name = name,
            ModifierGroupId = modifierGroupId
        };
    }

    public void Update(string name)
    {
        Name = name;
    }
}
