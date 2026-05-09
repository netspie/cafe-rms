using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Features.Orders;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Events;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class EventsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record EventDetail(
        Guid Id, string Name, string? Description, string? ImageUrl,
        Guid? ProductListId, Guid? PriceGroupId,
        string Status,
        DateTimeOffset? PublishedAt, DateTimeOffset? ClosedAt, DateTimeOffset? CancelledAt,
        string? CancellationReason,
        IReadOnlyList<DayInfo> Days,
        DateTimeOffset CreatedAt);
    private sealed record DayInfo(Guid Id, DateOnly Date);
    private sealed record EventItem(Guid Id, string Name, string Status, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<EventItem> Items, int Page, int PageSize, int Total);
    private sealed record AddResp(Guid Id);
    private sealed record AddDayResp(Guid Id);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);

        var response = await client.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_without_EventsManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Lifecycle_publish_then_close_with_attendance_bonus()
    {
        var ctx = await SeedAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);

        var add = await staff.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });
        var eventId = (await add.Content.ReadFromJsonAsync<AddResp>())!.Id;
        await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date = DateOnly.FromDateTime(DateTime.UtcNow) });

        (await staff.PostAsync($"/api/events/{eventId}/publish", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        await SeedOrderForUserAsync(ctx.OutletId, eventId, ctx.UserA);
        await SeedOrderForUserAsync(ctx.OutletId, eventId, ctx.UserB);
        await SeedOrderForUserAsync(ctx.OutletId, eventId, ctx.UserA);

        (await staff.PostAsync($"/api/events/{eventId}/close", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var balanceA = await db.LoyaltyPointLogs.Where(x => x.UserId == ctx.UserA).SumAsync(x => x.Points);
        var balanceB = await db.LoyaltyPointLogs.Where(x => x.UserId == ctx.UserB).SumAsync(x => x.Points);
        balanceA.Should().Be(CloseEvent.AttendanceBonusPoints);
        balanceB.Should().Be(CloseEvent.AttendanceBonusPoints);
    }

    [Test]
    public async Task Publish_with_no_days_returns_409()
    {
        await SeedAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);
        var add = await staff.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });
        var eventId = (await add.Content.ReadFromJsonAsync<AddResp>())!.Id;

        var response = await staff.PostAsync($"/api/events/{eventId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Close_before_publish_returns_409()
    {
        await SeedAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);
        var add = await staff.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });
        var eventId = (await add.Content.ReadFromJsonAsync<AddResp>())!.Id;
        await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date = DateOnly.FromDateTime(DateTime.UtcNow) });

        var response = await staff.PostAsync($"/api/events/{eventId}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Cancel_then_publish_returns_409()
    {
        await SeedAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);
        var add = await staff.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });
        var eventId = (await add.Content.ReadFromJsonAsync<AddResp>())!.Id;
        await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date = DateOnly.FromDateTime(DateTime.UtcNow) });
        await staff.PostAsJsonAsync($"/api/events/{eventId}/cancel", new { reason = "rain" });

        var response = await staff.PostAsync($"/api/events/{eventId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddDay_duplicate_date_returns_409()
    {
        await SeedAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);
        var add = await staff.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });
        var eventId = (await add.Content.ReadFromJsonAsync<AddResp>())!.Id;
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date });

        var response = await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task RemoveDay_returns_204_then_event_has_no_days()
    {
        await SeedAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);
        var add = await staff.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });
        var eventId = (await add.Content.ReadFromJsonAsync<AddResp>())!.Id;
        var dayResp = await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date = DateOnly.FromDateTime(DateTime.UtcNow) });
        var dayId = (await dayResp.Content.ReadFromJsonAsync<AddDayResp>())!.Id;

        var remove = await staff.DeleteAsync($"/api/events/{eventId}/days/{dayId}");
        remove.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await staff.GetAsync($"/api/events/{eventId}");
        var detail = await get.Content.ReadFromJsonAsync<EventDetail>();
        detail!.Days.Should().BeEmpty();
    }

    [Test]
    public async Task Update_after_close_returns_409()
    {
        await SeedAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);
        var add = await staff.PostAsJsonAsync("/api/events", new { name = "Jazz Night" });
        var eventId = (await add.Content.ReadFromJsonAsync<AddResp>())!.Id;
        await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date = DateOnly.FromDateTime(DateTime.UtcNow) });
        await staff.PostAsync($"/api/events/{eventId}/publish", null);
        await staff.PostAsync($"/api/events/{eventId}/close", null);

        var response = await staff.PutAsJsonAsync($"/api/events/{eventId}", new { name = "Renamed" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task GetById_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.EventsManage]);

        var response = await client.GetAsync($"/api/events/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record TestContext(Guid OutletId, Guid UserA, Guid UserB);

    private async Task<TestContext> SeedAsync()
    {
        var outlet = await factory.SeedOutletAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userA = AppUser.Create($"a-{Guid.NewGuid()}@test.local", "A", "User", AccountType.Guest);
        var userB = AppUser.Create($"b-{Guid.NewGuid()}@test.local", "B", "User", AccountType.Guest);
        db.Users.Add(userA);
        db.Users.Add(userB);
        await db.SaveChangesAsync();
        return new TestContext(outlet.Id, userA.Id, userB.Id);
    }

    private async Task SeedOrderForUserAsync(Guid outletId, Guid eventId, Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var order = Order.Create(outletId, userId: userId, eventId: eventId);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }
}
