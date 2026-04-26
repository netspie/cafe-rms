using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.PromotionCodes;

public class PromotionCode : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Code { get; private set; } = "";
    public decimal DiscountPercentage { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidUntil { get; private set; }
    public int? MaxUses { get; private set; }
    public int UsesCount { get; private set; }

    private PromotionCode() { }

    public static PromotionCode Create(
        string code,
        decimal discountPercentage,
        Guid companyId,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validUntil = null,
        int? maxUses = null)
    {
        return new PromotionCode
        {
            Id = Guid.NewGuid(),
            Code = code,
            DiscountPercentage = discountPercentage,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            MaxUses = maxUses,
            UsesCount = 0,
            CompanyId = companyId
        };
    }

    public void Update(
        string code,
        decimal discountPercentage,
        DateTimeOffset? validFrom,
        DateTimeOffset? validUntil,
        int? maxUses)
    {
        Code = code;
        DiscountPercentage = discountPercentage;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        MaxUses = maxUses;
    }
}
