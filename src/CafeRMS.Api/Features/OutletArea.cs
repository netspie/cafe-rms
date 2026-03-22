namespace CafeRMS.Api;

public class OutletArea
{
    public int Id { get; private init; }
    public string Name { get; set; }

    public OutletArea(string name)
    {
        Name = name;
    }
}