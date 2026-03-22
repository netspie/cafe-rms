namespace CafeRMS.Api;

public class Table
{
    public int Id { get; init; }
    public string Name { get; set; }
    public int AreaId { get; set; }

    public Table(int id, string name, int areaId)
    {
        Id = id;
        Name = name;
        AreaId = areaId;
    }
}