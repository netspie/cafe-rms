namespace CafeRMS.Api.Features;

public class SalesChannel
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
    public bool IsTakeout { get; set; }
}
