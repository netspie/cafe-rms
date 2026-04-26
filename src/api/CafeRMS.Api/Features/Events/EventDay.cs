using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Events;

public class EventDay : Entity
{
    public Guid Id { get; private init; }
    public Guid EventId { get; private init; }
    public Event? Event { get; private init; }
    public DateOnly Date { get; private set; }

    private EventDay() { }

    public static EventDay Create(Guid eventId, DateOnly date)
    {
        return new EventDay
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Date = date
        };
    }
}
