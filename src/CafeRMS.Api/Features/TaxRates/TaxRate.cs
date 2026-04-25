using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.TaxRates;

public class TaxRate : IAuditable, ISoftDeletable, ICompanyScoped
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public decimal Rate { get; private set; }
    public Guid CompanyId { get; private init; }
    public Company? Company { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private TaxRate() { }

    public static TaxRate Create(string name, string description, decimal rate, Guid companyId)
    {
        return new TaxRate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Rate = rate,
            CompanyId = companyId
        };
    }
}
