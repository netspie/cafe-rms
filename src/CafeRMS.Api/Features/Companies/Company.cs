using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Companies;

public class Company : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string Address { get; private set; } = "";
    public string TaxId { get; private set; } = "";
    public string Currency { get; private set; } = "";
    public string TimeZone { get; private set; } = "";

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Company() { }

    public static Company Create(string name, string address, string taxId, string currency, string timeZone, Guid createdBy)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Name = name,
            Address = address,
            TaxId = taxId,
            Currency = currency,
            TimeZone = timeZone,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
