using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.ModifierGroups;

public class ModifierGroup : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";

    private ModifierGroup() { }

    public static ModifierGroup Create(string name)
    {
        return new ModifierGroup
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
