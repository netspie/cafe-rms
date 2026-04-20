using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.SalesChannels;

public class SalesChannel : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public bool IsTakeout { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private SalesChannel() { }

    public static SalesChannel Create(string name, bool isTakeout)
    {
        return new SalesChannel
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsTakeout = isTakeout
        };
    }
}
