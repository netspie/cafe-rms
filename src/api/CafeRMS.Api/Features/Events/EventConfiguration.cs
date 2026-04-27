using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Events;

public class EventConfiguration : IEntityTypeConfiguration<Event>, IEntityTypeConfiguration<EventDay>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.IsPublished);
        builder.Ignore(x => x.IsClosed);
        builder.Ignore(x => x.IsCancelled);
        builder.Ignore(x => x.Status);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.HasOne(x => x.ProductList).WithMany().HasForeignKey(x => x.ProductListId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.PriceGroup).WithMany().HasForeignKey(x => x.PriceGroupId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<EventDay> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.EventId, x.Date }).IsUnique().HasDatabaseName("ix_event_days_event_id_date");
    }
}
