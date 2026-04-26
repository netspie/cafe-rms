using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.PriceGroups;

public class PriceGroup : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";

    private PriceGroup() { }

    public static PriceGroup Create(string name, Guid companyId)
    {
        return new PriceGroup
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
