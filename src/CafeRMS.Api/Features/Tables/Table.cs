using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Tables;

public class Table : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public Guid OutletId { get; private init; }
    public Outlet? Outlet { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Table() { }

    public static Table Create(string name, Guid outletId, Guid createdBy)
    {
        return new Table
        {
            Id = Guid.NewGuid(),
            Name = name,
            OutletId = outletId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
