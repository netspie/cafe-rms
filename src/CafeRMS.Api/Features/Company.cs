namespace CafeRMS.Api.Features;

public class Company
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
    public string Currency { get; init; } = "";
    public string TimeZone { get; init; } = "";
}
