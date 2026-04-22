using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.ProductLists;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Events;

public class Event : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public Guid? ProductListId { get; private init; }
    public ProductList? ProductList { get; private init; }
    public Guid? PriceGroupId { get; private init; }
    public PriceGroup? PriceGroup { get; private init; }
    public Guid CompanyId { get; private init; }
    public Company? Company { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

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
