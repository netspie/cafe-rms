using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeRMS.Api.Features.Products;

public class ProductConfiguration
    : IEntityTypeConfiguration<Product>,
      IEntityTypeConfiguration<ProductImage>,
      IEntityTypeConfiguration<ProductTag>,
      IEntityTypeConfiguration<ProductAllergen>,
      IEntityTypeConfiguration<ProductPrice>,
      IEntityTypeConfiguration<ProductModifierGroup>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Barcode).HasMaxLength(100);
        builder.HasOne(x => x.TaxRate).WithMany().HasForeignKey(x => x.TaxRateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(500);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.HasKey(x => new { x.ProductId, x.TagId });
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ProductAllergen> builder)
    {
        builder.HasKey(x => new { x.ProductId, x.AllergenId });
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Allergen).WithMany().HasForeignKey(x => x.AllergenId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Net).HasPrecision(18, 2);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PriceGroup).WithMany().HasForeignKey(x => x.PriceGroupId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ProductModifierGroup> builder)
    {
        builder.HasKey(x => new { x.ProductId, x.ModifierGroupId });
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ModifierGroup).WithMany().HasForeignKey(x => x.ModifierGroupId).OnDelete(DeleteBehavior.Cascade);
    }
}
