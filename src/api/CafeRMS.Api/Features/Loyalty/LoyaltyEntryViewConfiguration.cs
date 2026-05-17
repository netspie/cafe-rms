using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Loyalty;

public class LoyaltyEntryViewConfiguration : IEntityTypeConfiguration<LoyaltyEntryView>
{
    public void Configure(EntityTypeBuilder<LoyaltyEntryView> builder)
    {
        builder.HasNoKey().ToView("view_loyalty_entry");
        builder.Property(x => x.UserEmail).HasMaxLength(256);
        builder.Property(x => x.UserName).HasMaxLength(513);
        builder.Property(x => x.Reason).HasMaxLength(500);
    }
}
