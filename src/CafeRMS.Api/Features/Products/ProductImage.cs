using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Products;

public class ProductImage : Entity
{
    public Guid Id { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public string Url { get; private set; } = "";

    private ProductImage() { }

    public static ProductImage Create(Guid productId, string url)
    {
        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = url
        };
    }
}
