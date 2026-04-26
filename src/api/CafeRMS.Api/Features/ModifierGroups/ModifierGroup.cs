using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.ModifierGroups;

public class ModifierGroup : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";

    private ModifierGroup() { }

    public static ModifierGroup Create(string name, Guid companyId)
    {
        return new ModifierGroup
        {
            Id = Guid.NewGuid(),
            Name = name,
            CompanyId = companyId
        };
    }

    public void Update(string name)
    {
        Name = name;
    }
}
