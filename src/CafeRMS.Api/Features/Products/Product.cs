using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Products;

public class Product : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public string? Barcode { get; private set; }
    public Guid TaxRateId { get; private init; }
    public TaxRate? TaxRate { get; private init; }
    public Guid CompanyId { get; private init; }
    public Company? Company { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

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
