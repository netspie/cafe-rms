using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Products;

public class Product : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public string? Barcode { get; private set; }
    public Guid TaxRateId { get; private set; }
    public TaxRate? TaxRate { get; private init; }

    private Product() { }

    public static Product Create(string name, Guid taxRateId, string? description = null, string? barcode = null)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Barcode = barcode,
            TaxRateId = taxRateId
        };
    }

    public void Update(string name, string? description, string? barcode, Guid taxRateId)
    {
        Name = name;
        Description = description;
        Barcode = barcode;
        TaxRateId = taxRateId;
    }
}
