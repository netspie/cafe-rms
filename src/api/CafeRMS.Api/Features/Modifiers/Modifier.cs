using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Modifiers;

public class Modifier : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public decimal PriceDelta { get; private set; }
    public Guid ModifierGroupId { get; private init; }
    public ModifierGroup? ModifierGroup { get; private init; }

    private Modifier() { }

    public static Modifier Create(string name, Guid modifierGroupId, Guid companyId, decimal priceDelta = 0m)
    {
        return new Modifier
        {
            Id = Guid.NewGuid(),
            Name = name,
            PriceDelta = priceDelta,
            ModifierGroupId = modifierGroupId,
            CompanyId = companyId
        };
    }

    public void Update(string name, decimal priceDelta)
    {
        Name = name;
        PriceDelta = priceDelta;
    }
}
