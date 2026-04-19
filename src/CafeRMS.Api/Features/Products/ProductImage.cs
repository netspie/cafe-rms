using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Products;

public class ProductImage : IAuditable
{
    public Guid Id { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public string Url { get; private set; } = "";

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(Guid productId, string url, Guid createdBy)
    {
        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = url,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
