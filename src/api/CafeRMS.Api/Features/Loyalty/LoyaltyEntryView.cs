namespace CafeRMS.Api.Features.Loyalty;

public sealed class LoyaltyEntryView
{
    public Guid EntryId { get; init; }
    public Guid UserId { get; init; }
    public string? UserEmail { get; init; }
    public string? UserName { get; init; }
    public int Points { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
