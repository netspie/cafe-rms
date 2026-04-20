using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.Loyalty;

public class LoyaltyPointLog : IAuditable
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public AppUser? User { get; private init; }
    public int Points { get; private init; }
    public string? Reason { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private LoyaltyPointLog() { }

    public static LoyaltyPointLog Create(Guid userId, int points, string? reason = null)
    {
        return new LoyaltyPointLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Points = points,
            Reason = reason
        };
    }
}
