using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Products;

public class ProductPrice : Entity
{
    public Guid Id { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public Guid PriceGroupId { get; private init; }
    public PriceGroup? PriceGroup { get; private init; }
    public decimal Net { get; private set; }

    private ProductPrice() { }

    public static ProductPrice Create(Guid productId, Guid priceGroupId, decimal net)
    {
        return new ProductPrice
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            PriceGroupId = priceGroupId,
            Net = net
        };
    }
}
