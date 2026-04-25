namespace CafeRMS.Api.Shared.Entities;

public abstract class Entity : IAuditable
{
    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
}
