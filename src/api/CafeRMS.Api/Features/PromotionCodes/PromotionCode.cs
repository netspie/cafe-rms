using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.PromotionCodes;

public class PromotionCode : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Code { get; private set; } = "";
    public decimal DiscountPercentage { get; private set; }

    private PromotionCode() { }

    public static PromotionCode Create(string code, decimal discountPercentage, Guid companyId)
    {
        return new PromotionCode
        {
            Id = Guid.NewGuid(),
            Code = code,
            DiscountPercentage = discountPercentage,
            CompanyId = companyId
        };
    }
}
