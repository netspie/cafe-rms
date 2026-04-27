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

// Closed business process #1 — diagrams/1-activity-online-ordering.md
// Walks the full flow: register → place order with promo + loyalty redemption →
// staff accept → start preparing → mark ready → close → loyalty earned.
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
        DateTimeOffset? AcceptedAt, DateTimeOffset? InProgressAt, DateTimeOffset? ReadyAt,
        DateTimeOffset? ClosedAt, DateTimeOffset? CancelledAt, string? CancellationReason,
        IReadOnlyList<LineInfo> Lines, DateTimeOffset CreatedAt);
    private sealed record LineInfo(Guid Id, Guid ProductId, int Quantity, decimal NetPerOne, decimal VatPerOne);

    [Test]
    public async Task FullFlow_register_place_with_promo_and_loyalty_then_staff_lifecycle_to_close()
    {
        // ─── Setup: company + staff seed of catalog + promo (the staff side of the diagram). ───
        var company = await factory.SeedCompanyAsync();
        Guid outletId, productId;
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
            db.PromotionCodes.Add(PromotionCode.Create("WELCOME10", 10m, company.Id));
            await db.SaveChangesAsync();
        }

        // ─── Step 1: Klient — register (acts out the "Czy zalogowany? → Rejestracja" branch). ───
        using var anon = factory.CreateAnonymousClient();
        var reg = await anon.PostAsJsonAsync("/api/auth/register/guest", new
        {
            email = $"guest-{Guid.NewGuid()}@test.local",
            password = "GuestPass123!",
            firstName = "Anna",
            lastName = "Customer"
        });
        reg.StatusCode.Should().Be(HttpStatusCode.OK);

        // Pull the freshly-created user id so we can issue a guest JWT for the rest of the
        // flow + plant some loyalty points to redeem (the diagram's "wystarczający stan punktów" branch).
        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.OrderByDescending(x => x.CreatedAt).FirstAsync(x => x.AccountType == AccountType.Guest);
            userId = user.Id;
            db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(userId, 100, company.Id, "test seed"));
            await db.SaveChangesAsync();
        }

        using var guest = factory.CreateClientAs(AccountType.Guest, userId: userId);
        using var staff = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.OrdersManage, Permissions.OrdersView]);

        // ─── Step 2: Klient — place order with promo + loyalty redemption. ───
        var place = await guest.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            promotionCode = "WELCOME10",
            loyaltyPointsUsed = 30,
            lines = new[] { new { productId, quantity = 2, priceGroupId = (Guid?)null } }
        });
        place.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderId = (await place.Content.ReadFromJsonAsync<PlaceResp>())!.OrderId;

        // ─── Step 3: Pracownik — fulfil path: accept → start → ready → close. ───
        (await staff.PostAsync($"/api/orders/{orderId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await staff.PostAsync($"/api/orders/{orderId}/start-preparing", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await staff.PostAsync($"/api/orders/{orderId}/ready", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await staff.PostAsync($"/api/orders/{orderId}/close", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ─── Step 4: assertions on the closed-process invariants. ───
        var detailResp = await staff.GetAsync($"/api/orders/{orderId}");
        var detail = await detailResp.Content.ReadFromJsonAsync<OrderDetail>();
        detail!.Status.Should().Be("Closed");
        detail.PromotionCodeId.Should().NotBeNull();
        detail.LoyaltyPointsUsed.Should().Be(30);
        detail.Discount.Should().BeGreaterThan(0m);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Promo usage was incremented inside PlaceOrder.
            var promo = await db.PromotionCodes.IgnoreQueryFilters().FirstAsync(x => x.Code == "WELCOME10");
            promo.UsesCount.Should().Be(1);

            // Loyalty trail: +100 seed, -30 redemption (PlaceOrder), +N earn (CloseOrder).
            // Net should still be positive — earn formula is floor(net total - discount), with
            // discount applied against subtotal that includes VAT.
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
