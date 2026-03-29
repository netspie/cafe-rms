namespace CafeRMS.Api.Features;

public class Outlet
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
    public string Address { get; init; } = "";
    public int CompanyId { get; set; }
}
