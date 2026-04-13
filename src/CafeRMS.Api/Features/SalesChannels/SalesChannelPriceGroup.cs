using CafeRMS.Api.Features.PriceGroups;

namespace CafeRMS.Api.Features.SalesChannels;

public class SalesChannelPriceGroup
{
    public Guid SalesChannelId { get; private init; }
    public SalesChannel? SalesChannel { get; private init; }
    public Guid PriceGroupId { get; private init; }
    public PriceGroup? PriceGroup { get; private init; }

    private SalesChannelPriceGroup() { }

    public static SalesChannelPriceGroup Create(Guid salesChannelId, Guid priceGroupId)
    {
        return new SalesChannelPriceGroup
        {
            SalesChannelId = salesChannelId,
            PriceGroupId = priceGroupId
        };
    }
}
