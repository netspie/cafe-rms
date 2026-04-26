using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.SalesChannels;

public class SalesChannel : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public bool IsTakeout { get; private set; }

    private SalesChannel() { }

    public static SalesChannel Create(string name, bool isTakeout, Guid companyId)
    {
        return new SalesChannel
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsTakeout = isTakeout,
            CompanyId = companyId
        };
    }

    public void Update(string name, bool isTakeout)
    {
        Name = name;
        IsTakeout = isTakeout;
    }
}
