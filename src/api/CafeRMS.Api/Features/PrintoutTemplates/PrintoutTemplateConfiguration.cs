using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.PrintoutTemplates;

public class PrintoutTemplateConfiguration : IEntityTypeConfiguration<PrintoutTemplate>
{
    public void Configure(EntityTypeBuilder<PrintoutTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TemplateFileUrl).IsRequired().HasMaxLength(500);
        builder.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CompanyId, x.Name })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_printout_templates_company_id_name");
    }
}
