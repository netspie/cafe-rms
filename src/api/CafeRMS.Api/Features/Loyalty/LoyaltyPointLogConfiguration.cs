using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Loyalty;

public class LoyaltyPointLogConfiguration : IEntityTypeConfiguration<LoyaltyPointLog>
{
    public void Configure(EntityTypeBuilder<LoyaltyPointLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
