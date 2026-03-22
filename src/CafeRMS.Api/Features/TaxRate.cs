namespace CafeRMS.Api;

public class TaxRate
{
    public int Id { get; init; }
    public string Name { get; set; }
    public decimal Rate { get; set; }

    public TaxRate(int id, string name, decimal rate)
    {
        Id = id;
        Name = name;
        Rate = rate;
    }
}