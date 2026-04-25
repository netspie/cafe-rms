namespace CafeRMS.Api.Shared.Entities;

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; }
    Guid? DeletedBy { get; }
}
