using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.TaxRates;

public class TaxRate : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public decimal Rate { get; private set; }

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

    public void Update(string name, string description, decimal rate)
    {
        Name = name;
        Description = description;
        Rate = rate;
    }
}
