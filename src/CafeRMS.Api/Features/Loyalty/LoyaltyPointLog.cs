using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.Loyalty;

public class LoyaltyPointLog : CompanyOwnedEntity
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public AppUser? User { get; private init; }
    public int Points { get; private init; }
    public string? Reason { get; private init; }

    private LoyaltyPointLog() { }

    public static LoyaltyPointLog Create(Guid userId, int points, Guid companyId, string? reason = null)
    {
        return new LoyaltyPointLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Points = points,
            Reason = reason,
            CompanyId = companyId
        };
    }
}
