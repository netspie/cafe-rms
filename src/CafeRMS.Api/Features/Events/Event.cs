using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.ProductLists;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Events;

public class Event : CompanyOwnedEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public Guid? ProductListId { get; private init; }
    public ProductList? ProductList { get; private init; }
    public Guid? PriceGroupId { get; private init; }
    public PriceGroup? PriceGroup { get; private init; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public bool IsCancelled => CancelledAt is not null;

    private Event() { }

    public static Event Create(string name, Guid companyId, string? description = null, string? imageUrl = null,
        Guid? productListId = null, Guid? priceGroupId = null)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ImageUrl = imageUrl,
            ProductListId = productListId,
            PriceGroupId = priceGroupId,
            CompanyId = companyId
        };
    }
}
