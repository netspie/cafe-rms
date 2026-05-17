using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.PromotionCodes;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.E2E;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class OnlineOrderingE2ETests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record PlaceResp(Guid OrderId);
    private sealed record OrderDetail(
        Guid Id, Guid OutletId, Guid? TableId, Guid? SalesChannelId, Guid? UserId, Guid? EventId,
        Guid? PromotionCodeId, decimal Discount, int LoyaltyPointsUsed, string Status,
        DateTimeOffset? ClosedAt, DateTimeOffset? CancelledAt, string? CancellationReason,
        IReadOnlyList<LineInfo> Lines, DateTimeOffset CreatedAt);
    private sealed record LineInfo(Guid Id, Guid ProductId, int Quantity, decimal NetPerOne, decimal VatPerOne);

    [Test]
    public async Task FullFlow_register_place_with_promo_and_loyalty_then_staff_lifecycle_to_close()
    {
        var outlet = await factory.SeedOutletAsync();
        Guid outletId = outlet.Id;
        Guid productId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var taxRate = TaxRate.Create("VAT 23%", "Standard", 23m);
            db.TaxRates.Add(taxRate);
            var product = Product.Create("Espresso", taxRate.Id);
            db.Products.Add(product);
            productId = product.Id;
            var pg = PriceGroup.Create("Standard");
            db.PriceGroups.Add(pg);
            db.ProductPrices.Add(ProductPrice.Create(product.Id, pg.Id, 5.00m));
            db.PromotionCodes.Add(PromotionCode.Create("WELCOME10", 10m));
            await db.SaveChangesAsync();
        }

        using var anon = factory.CreateAnonymousClient();
        var reg = await anon.PostAsJsonAsync("/api/auth/register/guest", new
        {
            email = $"guest-{Guid.NewGuid()}@test.local",
            password = "GuestPass123!",
            firstName = "Anna",
            lastName = "Customer"
        });
        reg.StatusCode.Should().Be(HttpStatusCode.OK);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.OrderByDescending(x => x.CreatedAt).FirstAsync(x => x.AccountType == AccountType.Guest);
            userId = user.Id;
            db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(userId, 100, "test seed"));
            await db.SaveChangesAsync();
        }

        using var guest = factory.CreateClientAs(AccountType.Guest, userId: userId);
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.OrdersManage, Permissions.OrdersView]);

        var place = await guest.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            promotionCode = "WELCOME10",
            loyaltyPointsUsed = 30,
            lines = new[] { new { productId, quantity = 2, priceGroupId = (Guid?)null } }
        });
        place.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderId = (await place.Content.ReadFromJsonAsync<PlaceResp>())!.OrderId;

        (await staff.PostAsync($"/api/orders/{orderId}/close", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var detailResp = await staff.GetAsync($"/api/orders/{orderId}");
        var detail = await detailResp.Content.ReadFromJsonAsync<OrderDetail>();
        detail!.Status.Should().Be("Closed");
        detail.PromotionCodeId.Should().NotBeNull();
        detail.LoyaltyPointsUsed.Should().Be(30);
        detail.Discount.Should().BeGreaterThan(0m);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var promo = await db.PromotionCodes.IgnoreQueryFilters().FirstAsync(x => x.Code == "WELCOME10");
            promo.UsesCount.Should().Be(1);

            var entries = await db.LoyaltyPointLogs.IgnoreQueryFilters()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
            entries.Should().HaveCount(3);
            entries.Select(x => x.Points).Sum().Should().BeGreaterThan(0);
            entries.Should().Contain(x => x.Points == -30);
            entries.Should().Contain(x => x.Points > 0 && x.Reason!.Contains("Earned"));
        }
    }
}
