using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.PromotionCodes;

public class PromotionCode : IAuditable, ISoftDeletable
{
    public Guid Id { get; private init; }
    public string Code { get; private set; } = "";
    public decimal DiscountPercentage { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private PromotionCode() { }

    public static PromotionCode Create(string code, decimal discountPercentage, Guid createdBy)
    {
        return new PromotionCode
        {
            Id = Guid.NewGuid(),
            Code = code,
            DiscountPercentage = discountPercentage,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
