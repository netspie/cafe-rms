using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Tables;

public class Table : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public Guid OutletId { get; private init; }
    public Outlet? Outlet { get; private init; }

    private Table() { }

    public static Table Create(string name, Guid outletId)
    {
        return new Table
        {
            Id = Guid.NewGuid(),
            Name = name,
            OutletId = outletId
        };
    }

    public void Update(string name)
    {
        Name = name;
    }
}
