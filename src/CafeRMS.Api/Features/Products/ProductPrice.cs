using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Products;

public class ProductPrice : IAuditable
{
    public Guid Id { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public Guid PriceGroupId { get; private init; }
    public PriceGroup? PriceGroup { get; private init; }
    public decimal Net { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid? CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private ProductPrice() { }

    public static ProductPrice Create(Guid productId, Guid priceGroupId, decimal net, Guid? createdBy = null)
    {
        return new ProductPrice
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            PriceGroupId = priceGroupId,
            Net = net,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
