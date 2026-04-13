using CafeRMS.Api.Features.Tags;

namespace CafeRMS.Api.Features.Products;

public class ProductTag
{
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public Guid TagId { get; private init; }
    public Tag? Tag { get; private init; }

    private ProductTag() { }

    public static ProductTag Create(Guid productId, Guid tagId)
    {
        return new ProductTag { ProductId = productId, TagId = tagId };
    }
}
