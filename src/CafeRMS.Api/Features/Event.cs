namespace CafeRMS.Api.Features;

public class Event
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public int? ProductListId { get; set; }
    public int? PriceGroupId { get; set; }
}

public class EventDay
{
    public int Id { get; private init; }
    public int EventId { get; set; }
    public DateTime Date { get; set; }
}
