using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Events;

public class EventDay : IAuditable
{
    public Guid Id { get; private init; }
    public Guid EventId { get; private init; }
    public Event? Event { get; private init; }
    public DateOnly Date { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid? CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private EventDay() { }

    public static EventDay Create(Guid eventId, DateOnly date, Guid? createdBy = null)
    {
        return new EventDay
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Date = date,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
