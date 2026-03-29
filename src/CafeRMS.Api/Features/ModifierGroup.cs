namespace CafeRMS.Api.Features;

public class ModifierGroup
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
}

public class Modifier
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
    public int ModifierGroupId { get; set; }
}
