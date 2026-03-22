namespace CafeRMS.Api;

public class Outlet
{
    public int Id { get; init; }
    public string Name { get; set; }
    public string Address { get; set; }
    public int CompanyId { get; set; }

    public Outlet(int id, string name, string address, int companyId)
    {
        Id = id;
        Name = name;
        Address = address;
        CompanyId = companyId;
    }
}