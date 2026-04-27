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
    public Guid? ProductListId { get; private set; }
    public ProductList? ProductList { get; private init; }
    public Guid? PriceGroupId { get; private set; }
    public PriceGroup? PriceGroup { get; private init; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public bool IsPublished => PublishedAt is not null;
    public bool IsClosed => ClosedAt is not null;
    public bool IsCancelled => CancelledAt is not null;

    public EventStatus Status =>
        IsCancelled ? EventStatus.Cancelled
        : IsClosed ? EventStatus.Closed
        : IsPublished ? EventStatus.Published
        : EventStatus.Draft;

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

    public void Update(string name, string? description, string? imageUrl, Guid? productListId, Guid? priceGroupId)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        ProductListId = productListId;
        PriceGroupId = priceGroupId;
    }

    public void Publish(DateTimeOffset now) => PublishedAt = now;
    public void Close(DateTimeOffset now) => ClosedAt = now;
    public void Cancel(DateTimeOffset now, string? reason)
    {
        CancelledAt = now;
        CancellationReason = reason;
    }
}
