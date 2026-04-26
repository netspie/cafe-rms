using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.ProductLists;

public class ProductList : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";

    private ProductList() { }

    public static ProductList Create(string name, Guid companyId)
    {
        return new ProductList
        {
            Id = Guid.NewGuid(),
            Name = name,
            CompanyId = companyId
        };
    }
}
