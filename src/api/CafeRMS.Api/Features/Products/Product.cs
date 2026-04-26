using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Products;

public class Product : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public string? Barcode { get; private set; }
    public Guid TaxRateId { get; private init; }
    public TaxRate? TaxRate { get; private init; }

    private Product() { }

    public static Product Create(string name, Guid taxRateId, Guid companyId, string? description = null, string? barcode = null)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Barcode = barcode,
            TaxRateId = taxRateId,
            CompanyId = companyId
        };
    }
}
