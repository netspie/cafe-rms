namespace CafeRMS.Api;

public class Allergen
{
    public int Id { get; init; }
    public string Name { get; set; }

    public Allergen(string name)
    {
        Name = name;
    }
}