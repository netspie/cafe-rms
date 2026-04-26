using CafeRMS.Api.Features.Allergens;

namespace CafeRMS.Api.Features.Products;

public class ProductAllergen
{
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }
    public Guid AllergenId { get; private init; }
    public Allergen? Allergen { get; private init; }

    private ProductAllergen() { }

    public static ProductAllergen Create(Guid productId, Guid allergenId)
    {
        return new ProductAllergen { ProductId = productId, AllergenId = allergenId };
    }
}
