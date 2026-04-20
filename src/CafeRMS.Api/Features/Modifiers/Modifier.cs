using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Modifiers;

public class Modifier : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public Guid ModifierGroupId { get; private init; }
    public ModifierGroup? ModifierGroup { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

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
}
