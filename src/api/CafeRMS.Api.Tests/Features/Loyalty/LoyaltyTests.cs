using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Loyalty;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class LoyaltyTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record BalanceResp(int Balance);
    private sealed record HistoryItem(Guid Id, int Points, string? Reason, DateTimeOffset CreatedAt);
    private sealed record StaffEntry(Guid Id, Guid UserId, int Points, string? Reason, DateTimeOffset CreatedAt);
    private sealed record PageDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
    private sealed record AddResp(Guid Id);

    [Test]
    public async Task GetBalance_sums_user_log()
    {
        var userId = await SeedGuestAsync();
        await SeedLogAsync(userId, 50, "earn");
        await SeedLogAsync(userId, -20, "redeem");
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.GetAsync("/api/my/loyalty/balance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BalanceResp>();
        body!.Balance.Should().Be(30);
    }

    [Test]
    public async Task GetHistory_returns_own_entries_only()
    {
        var userA = await SeedGuestAsync();
        var userB = await SeedGuestAsync();
        await SeedLogAsync(userA, 10, "A1");
        await SeedLogAsync(userB, 99, "B1");
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userA);

        var response = await client.GetAsync("/api/my/loyalty/history");

        var body = await response.Content.ReadFromJsonAsync<PageDto<HistoryItem>>();
        body!.Items.Should().HaveCount(1);
        body.Items[0].Reason.Should().Be("A1");
    }

    [Test]
    public async Task AddAdjustment_happy_path_returns_id()
    {
        var userId = await SeedGuestAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.LoyaltyManage]);

        var response = await staff.PostAsJsonAsync("/api/loyalty/entries", new { userId, points = 100, reason = "Comp for spilled coffee" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task AddAdjustment_zero_points_returns_400()
    {
        var userId = await SeedGuestAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.LoyaltyManage]);

        var response = await staff.PostAsJsonAsync("/api/loyalty/entries", new { userId, points = 0, reason = "noop" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AddAdjustment_without_LoyaltyManage_returns_403()
    {
        var userId = await SeedGuestAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await staff.PostAsJsonAsync("/api/loyalty/entries", new { userId, points = 10, reason = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Guid> SeedGuestAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = AppUser.Create($"guest-{Guid.NewGuid()}@test.local", "Guest", "User", AccountType.Guest);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private async Task SeedLogAsync(Guid userId, int points, string reason)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(userId, points, reason));
        await db.SaveChangesAsync();
    }
}
