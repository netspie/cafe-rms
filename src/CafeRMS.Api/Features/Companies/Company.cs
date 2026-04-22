using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Companies;

public class Company : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string LegalName { get; private set; } = "";
    public string TaxId { get; private set; } = "";
    public string InvoicingAddress { get; private set; } = "";
    public string BillingEmail { get; private set; } = "";
    public string BillingPhone { get; private set; } = "";

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Company() { }

    public static Company Create(string legalName, string taxId, string invoicingAddress, string billingEmail, string billingPhone)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            LegalName = legalName,
            TaxId = taxId,
            InvoicingAddress = invoicingAddress,
            BillingEmail = billingEmail,
            BillingPhone = billingPhone
        };
    }
}
