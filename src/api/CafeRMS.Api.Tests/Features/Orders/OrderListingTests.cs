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
public sealed class OrderListingTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record OrderItem(Guid Id, Guid OutletId, Guid? UserId, string Status, DateTimeOffset CreatedAt, DateTimeOffset? ClosedAt);
    private sealed record PageDto(IReadOnlyList<OrderItem> Items, int Page, int PageSize, int Total);
    private sealed record OrderDetail(
        Guid Id, Guid OutletId, Guid? TableId, Guid? SalesChannelId, Guid? UserId, Guid? EventId,
        Guid? PromotionCodeId, decimal Discount, int LoyaltyPointsUsed, string Status,
        DateTimeOffset? AcceptedAt, DateTimeOffset? InProgressAt, DateTimeOffset? ReadyAt,
        DateTimeOffset? ClosedAt, DateTimeOffset? CancelledAt, string? CancellationReason,
        IReadOnlyList<LineInfo> Lines, DateTimeOffset CreatedAt);
    private sealed record LineInfo(Guid Id, Guid ProductId, int Quantity, decimal NetPerOne, decimal VatPerOne);
    private sealed record PlaceResp(Guid OrderId);

    [Test]
    public async Task Staff_list_returns_company_orders_only()
    {
        var ownCtx = await SeedAndPlaceAsync(companyName: "Own", taxId: "1");
        var otherCtx = await SeedAndPlaceAsync(companyName: "Other", taxId: "2");
        using var staff = factory.CreateClientAs(AccountType.Staff, companyId: ownCtx.CompanyId, permissions: [Permissions.OrdersView]);

        var response = await staff.GetAsync("/api/orders");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(1);
        body.Items[0].Id.Should().Be(ownCtx.OrderId);
    }

    [Test]
    public async Task Staff_GetById_returns_order_with_lines()
    {
        var ctx = await SeedAndPlaceAsync();
        using var staff = factory.CreateClientAs(AccountType.Staff, companyId: ctx.CompanyId, permissions: [Permissions.OrdersView]);

        var response = await staff.GetAsync($"/api/orders/{ctx.OrderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderDetail>();
        body!.Lines.Should().HaveCount(1);
        body.Status.Should().Be(nameof(OrderStatus.Placed));
    }

    [Test]
    public async Task Customer_my_orders_returns_only_own()
    {
        var ctx = await SeedAndPlaceAsync();
        using var guest = factory.CreateClientAs(AccountType.Guest, userId: ctx.UserId);

        var response = await guest.GetAsync("/api/my/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(1);
        body.Items[0].Id.Should().Be(ctx.OrderId);
    }

    [Test]
    public async Task Customer_GetMyOrderById_other_users_order_returns_404()
    {
        var ctx = await SeedAndPlaceAsync();
        using var stranger = factory.CreateClientAs(AccountType.Guest, userId: Guid.NewGuid());

        var response = await stranger.GetAsync($"/api/my/orders/{ctx.OrderId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record TestContext(Guid CompanyId, Guid UserId, Guid OrderId);

    private async Task<TestContext> SeedAndPlaceAsync(string companyName = "Test", string taxId = "0000000000")
    {
        var company = await factory.SeedCompanyAsync(legalName: companyName, taxId: taxId);
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
            var user = AppUser.Create($"guest-{Guid.NewGuid()}@test.local", "Guest", "User", AccountType.Guest, null);
            db.Users.Add(user);
            userId = user.Id;
            await db.SaveChangesAsync();
        }
        var guest = factory.CreateClientAs(AccountType.Guest, userId: userId);
        var place = await guest.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId, quantity = 1, priceGroupId = (Guid?)null } }
        });
        var resp = await place.Content.ReadFromJsonAsync<PlaceResp>();
        return new TestContext(company.Id, userId, resp!.OrderId);
    }
}
