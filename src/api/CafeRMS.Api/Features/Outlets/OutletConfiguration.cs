using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Outlets;

public class OutletConfiguration : IEntityTypeConfiguration<Outlet>
{
    public void Configure(EntityTypeBuilder<Outlet> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.StreetAddress).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(50);
        builder.Property(x => x.TimeZone).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Currency).HasConversion<string>().HasMaxLength(5);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
    }
}
