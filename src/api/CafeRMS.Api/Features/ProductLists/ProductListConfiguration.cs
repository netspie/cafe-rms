using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.ProductLists;

public class ProductListConfiguration : IEntityTypeConfiguration<ProductList>, IEntityTypeConfiguration<ProductListItem>
{
    public void Configure(EntityTypeBuilder<ProductList> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_product_lists_name");
    }

    public void Configure(EntityTypeBuilder<ProductListItem> builder)
    {
        builder.HasKey(x => new { x.ProductListId, x.ProductId });
        builder.HasOne(x => x.ProductList).WithMany().HasForeignKey(x => x.ProductListId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
