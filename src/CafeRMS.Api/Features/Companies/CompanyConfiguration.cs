using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Companies;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LegalName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TaxId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.InvoicingAddress).IsRequired().HasMaxLength(500);
        builder.Property(x => x.BillingEmail).IsRequired().HasMaxLength(256);
        builder.Property(x => x.BillingPhone).IsRequired().HasMaxLength(50);
    }
}
