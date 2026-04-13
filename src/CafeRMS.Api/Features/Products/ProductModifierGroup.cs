using CafeRMS.Api.Features.ModifierGroups;

namespace CafeRMS.Api.Features.Products;

public class ProductModifierGroup
{
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public Guid ModifierGroupId { get; private init; }
    public ModifierGroup? ModifierGroup { get; private init; }

    private ProductModifierGroup() { }

    public static ProductModifierGroup Create(Guid productId, Guid modifierGroupId)
    {
        return new ProductModifierGroup { ProductId = productId, ModifierGroupId = modifierGroupId };
    }
}
