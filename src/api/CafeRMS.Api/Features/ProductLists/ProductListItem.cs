using CafeRMS.Api.Features.Products;

namespace CafeRMS.Api.Features.ProductLists;

public class ProductListItem
{
    public Guid ProductListId { get; private init; }
    public ProductList? ProductList { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }

    private ProductListItem() { }

    public static ProductListItem Create(Guid productListId, Guid productId)
    {
        return new ProductListItem { ProductListId = productListId, ProductId = productId };
    }
}
