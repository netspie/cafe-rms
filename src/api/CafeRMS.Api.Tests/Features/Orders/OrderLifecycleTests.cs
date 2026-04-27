using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Orders;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class OrderLifecycleTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record OrderDetail(
        Guid Id, Guid OutletId, Guid? TableId, Guid? SalesChannelId, Guid? UserId, Guid? EventId,
        Guid? PromotionCodeId, decimal Discount, int LoyaltyPointsUsed, string Status,
        DateTimeOffset? AcceptedAt, DateTimeOffset? InProgressAt, DateTimeOffset? ReadyAt,
        DateTimeOffset? ClosedAt, DateTimeOffset? CancelledAt, string? CancellationReason,
        IReadOnlyList<LineInfo> Lines, DateTimeOffset CreatedAt);
    private sealed record LineInfo(Guid Id, Guid ProductId, int Quantity, decimal NetPerOne, decimal VatPerOne);

    [Test]
    public async Task Lifecycle_accept_then_start_then_ready_then_close()
    {
        var ctx = await SeedAndPlaceAsync();

        (await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/start-preparing", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/ready", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/close", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await ctx.Staff.GetAsync($"/api/orders/{ctx.OrderId}");
        var body = await get.Content.ReadFromJsonAsync<OrderDetail>();
        body!.Status.Should().Be(nameof(OrderStatus.Closed));
    }

    [Test]
    public async Task StartPreparing_before_accept_returns_409()
    {
        var ctx = await SeedAndPlaceAsync();

        var response = await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/start-preparing", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Close_before_ready_returns_409()
    {
        var ctx = await SeedAndPlaceAsync();
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/accept", null);
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/start-preparing", null);

        var response = await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Close_already_closed_returns_409()
    {
        var ctx = await SeedAndPlaceAsync();
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/accept", null);
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/start-preparing", null);
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/ready", null);
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/close", null);

        var response = await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Close_credits_loyalty_points()
    {
        var ctx = await SeedAndPlaceAsync();
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/accept", null);
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/start-preparing", null);
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/ready", null);

        var close = await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/close", null);
        close.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var balance = await db.LoyaltyPointLogs.IgnoreQueryFilters().Where(x => x.UserId == ctx.UserId).SumAsync(x => x.Points);
        // Net per line = 5.00, qty 2 → 10.00 net total → 10 points.
        balance.Should().Be(10);
    }

    [Test]
    public async Task Customer_cancel_before_accept_succeeds()
    {
        var ctx = await SeedAndPlaceAsync();

        var response = await ctx.Guest.PostAsJsonAsync($"/api/my/orders/{ctx.OrderId}/cancel", new { reason = "changed mind" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Customer_cancel_after_accept_returns_403()
    {
        var ctx = await SeedAndPlaceAsync();
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/accept", null);

        var response = await ctx.Guest.PostAsJsonAsync($"/api/my/orders/{ctx.OrderId}/cancel", new { reason = "too late" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Staff_cancel_after_accept_succeeds()
    {
        var ctx = await SeedAndPlaceAsync();
        await ctx.Staff.PostAsync($"/api/orders/{ctx.OrderId}/accept", null);

        var response = await ctx.Staff.PostAsJsonAsync($"/api/orders/{ctx.OrderId}/cancel", new { reason = "kitchen issue" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Customer_cancel_other_users_order_returns_404()
    {
        var ctx = await SeedAndPlaceAsync();
        var strangerId = Guid.NewGuid();
        using var stranger = factory.CreateClientAs(AccountType.Guest, userId: strangerId);

        var response = await stranger.PostAsJsonAsync($"/api/my/orders/{ctx.OrderId}/cancel", new { reason = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record TestContext(Guid CompanyId, Guid UserId, Guid OutletId, Guid OrderId, HttpClient Staff, HttpClient Guest);

    private async Task<TestContext> SeedAndPlaceAsync()
    {
        var company = await factory.SeedCompanyAsync();
        Guid outletId, productId, userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var outlet = await db.Outlets.IgnoreQueryFilters().FirstAsync(x => x.CompanyId == company.Id);
            outletId = outlet.Id;
            var taxRate = TaxRate.Create("VAT 23%", "Standard", 23m, company.Id);
            db.TaxRates.Add(taxRate);
            var product = Product.Create("Espresso", taxRate.Id, company.Id);
            db.Products.Add(product);
            productId = product.Id;
            var pg = PriceGroup.Create("Standard", company.Id);
            db.PriceGroups.Add(pg);
            db.ProductPrices.Add(ProductPrice.Create(product.Id, pg.Id, 5.00m));
            var user = AppUser.Create("guest@test.local", "Guest", "User", AccountType.Guest, null);
            db.Users.Add(user);
            userId = user.Id;
            await db.SaveChangesAsync();
        }
        var staff = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.OrdersManage, Permissions.OrdersView]);
        var guest = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var place = await guest.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId, quantity = 2, priceGroupId = (Guid?)null } }
        });
        var resp = await place.Content.ReadFromJsonAsync<PlaceResp>();
        return new TestContext(company.Id, userId, outletId, resp!.OrderId, staff, guest);
    }

    private sealed record PlaceResp(Guid OrderId);
}
