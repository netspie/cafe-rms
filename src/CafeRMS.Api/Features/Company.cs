namespace CafeRMS.Api;

public class Company
{
    public int Id { get; private init; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string TaxId { get; set; }

    public Company(int id, string name, string address, string taxId)
    {
        Id = id;
        Name = name;
        Address = address;
        TaxId = taxId;
    }
}