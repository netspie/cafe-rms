namespace CafeRMS.Api.Features;

public class Tag
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
    public string ImageUrl { get; init; } = "";
}
