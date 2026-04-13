using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products;

namespace CafeRMS.Api.Features.Favorites;

public class Favorite
{
    public Guid UserId { get; private init; }
    public AppUser? User { get; private init; }
    public Guid ProductId { get; private init; }
    public Product? Product { get; private init; }

    private Favorite() { }

    public static Favorite Create(Guid userId, Guid productId)
    {
        return new Favorite { UserId = userId, ProductId = productId };
    }
}
