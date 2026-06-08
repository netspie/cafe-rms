using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Outlets;

public class Outlet : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string DisplayName { get; private set; } = "";
    public string StreetAddress { get; private set; } = "";
    public string Phone { get; private set; } = "";
    public string TimeZone { get; private set; } = "";
    public Currency Currency { get; private set; }
    public string? LogoUrl { get; private set; }
    public string LegalName { get; private set; } = "";
    public string TaxId { get; private set; } = "";
    public string InvoicingAddress { get; private set; } = "";
    public string BillingEmail { get; private set; } = "";
    public string BillingPhone { get; private set; } = "";

    private Outlet() { }

    public static Outlet Create(
        string displayName,
        string streetAddress,
        string phone,
        string timeZone,
        Currency currency,
        string legalName,
        string taxId,
        string invoicingAddress,
        string billingEmail,
        string billingPhone,
        string? logoUrl = null)
    {
        return new Outlet
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            StreetAddress = streetAddress,
            Phone = phone,
            TimeZone = timeZone,
            Currency = currency,
            LogoUrl = logoUrl,
            LegalName = legalName,
            TaxId = taxId,
            InvoicingAddress = invoicingAddress,
            BillingEmail = billingEmail,
            BillingPhone = billingPhone
        };
    }

    public void Update(
        string displayName,
        string streetAddress,
        string phone,
        string timeZone,
        Currency currency,
        string legalName,
        string taxId,
        string invoicingAddress,
        string billingEmail,
        string billingPhone,
        string? logoUrl)
    {
        DisplayName = displayName;
        StreetAddress = streetAddress;
        Phone = phone;
        TimeZone = timeZone;
        Currency = currency;
        LogoUrl = logoUrl;
        LegalName = legalName;
        TaxId = taxId;
        InvoicingAddress = invoicingAddress;
        BillingEmail = billingEmail;
        BillingPhone = billingPhone;
    }
}
