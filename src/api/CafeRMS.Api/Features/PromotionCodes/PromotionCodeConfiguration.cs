using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.PromotionCodes;

public class PromotionCodeConfiguration : IEntityTypeConfiguration<PromotionCode>
{
    public void Configure(EntityTypeBuilder<PromotionCode> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DiscountPercentage).HasPrecision(18, 2);
        builder.Property(x => x.UsesCount).HasDefaultValue(0);
        builder.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);

        // Partial unique index — soft-deleted codes can have their string reused.
        builder.HasIndex(x => new { x.CompanyId, x.Code })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_promotion_codes_company_id_code");
    }
}
