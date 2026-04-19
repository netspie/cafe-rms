using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.ProductLists;

public class ProductList : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private ProductList() { }

    public static ProductList Create(string name, Guid createdBy)
    {
        return new ProductList
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
