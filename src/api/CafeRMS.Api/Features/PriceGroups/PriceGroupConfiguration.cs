using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.PriceGroups;

public class PriceGroupConfiguration : IEntityTypeConfiguration<PriceGroup>
{
    public void Configure(EntityTypeBuilder<PriceGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_price_groups_name");
    }
}
