namespace CafeRMS.Api.Features;

public class LoyaltyPointLog
{
    public int Id { get; private init; }
    public int UserId { get; set; }
    public int Points { get; set; }
}

public class Favorite
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
}
