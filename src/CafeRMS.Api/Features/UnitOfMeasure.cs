namespace CafeRMS.Api;

public class UnitOfMeasure
{
    public int Id { get; init; }
    public string Name { get; set; }
    public string ShortName { get; set; }

    public UnitOfMeasure(int id, string name, string shortName)
    {
        Id = id;
        Name = name;
        ShortName = shortName;
    }
}