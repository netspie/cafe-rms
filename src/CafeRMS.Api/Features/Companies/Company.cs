using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Companies;

public class Company : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string LegalName { get; private set; } = "";
    public string TaxId { get; private set; } = "";
    public string InvoicingAddress { get; private set; } = "";
    public string BillingEmail { get; private set; } = "";
    public string BillingPhone { get; private set; } = "";
    public bool IsPublic { get; set; }

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

    public void Update(string legalName, string taxId, string invoicingAddress, string billingEmail, string billingPhone, bool isPublic)
    {
        LegalName = legalName;
        TaxId = taxId;
        InvoicingAddress = invoicingAddress;
        BillingEmail = billingEmail;
        BillingPhone = billingPhone;
        IsPublic = isPublic;
    }
}
