using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>, IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.IsClosed);
        builder.Ignore(x => x.IsCancelled);
        builder.Ignore(x => x.Status);
        builder.Property(x => x.Discount).HasPrecision(18, 2);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.HasOne(x => x.Outlet).WithMany().HasForeignKey(x => x.OutletId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Table).WithMany().HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.SalesChannel).WithMany().HasForeignKey(x => x.SalesChannelId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.PromotionCode).WithMany().HasForeignKey(x => x.PromotionCodeId).OnDelete(DeleteBehavior.SetNull);
    }

    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NetPerOne).HasPrecision(18, 2);
        builder.Property(x => x.VatPerOne).HasPrecision(18, 2);
        builder.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
