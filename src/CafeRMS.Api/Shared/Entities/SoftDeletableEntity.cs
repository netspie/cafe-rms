namespace CafeRMS.Api.Shared.Entities;

public abstract class SoftDeletableEntity : Entity, ISoftDeletable
{
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
}
