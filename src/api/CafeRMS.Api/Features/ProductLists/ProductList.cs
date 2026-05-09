using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.ProductLists;

public class ProductList : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";

    private ProductList() { }

    public static ProductList Create(string name)
    {
        return new ProductList
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void Update(string name)
    {
        Name = name;
    }
}
