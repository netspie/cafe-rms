using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.SalesChannels;

public class SalesChannelConfiguration : IEntityTypeConfiguration<SalesChannel>, IEntityTypeConfiguration<SalesChannelPriceGroup>
{
    public void Configure(EntityTypeBuilder<SalesChannel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }

    public void Configure(EntityTypeBuilder<SalesChannelPriceGroup> builder)
    {
        builder.HasKey(x => new { x.SalesChannelId, x.PriceGroupId });
        builder.HasOne(x => x.SalesChannel).WithMany().HasForeignKey(x => x.SalesChannelId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PriceGroup).WithMany().HasForeignKey(x => x.PriceGroupId).OnDelete(DeleteBehavior.Cascade);
    }
}
