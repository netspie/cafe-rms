namespace CafeRMS.Api.Features;

public class Table
{
    public int Id { get; private init; }
    public string Code { get; init; } = "";
    public int OutletId { get; set; }
}
