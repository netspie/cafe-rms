using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Orders;

public class OrderLineSummaryConfiguration : IEntityTypeConfiguration<OrderLineSummary>
{
    public void Configure(EntityTypeBuilder<OrderLineSummary> builder)
    {
        builder.HasNoKey().ToView("vw_order_lines_summary");
        builder.Property(x => x.NetPerOne).HasPrecision(18, 2);
        builder.Property(x => x.VatPerOne).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);
    }
}
