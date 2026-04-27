using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Events.UseCases;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.E2E;

// Closed business process #3 — diagrams/3-activity-event-management.md
// Walks the full flow: staff sets up product + event-pricing → creates Event tied to a
// dedicated ProductList + PriceGroup → adds days → publishes → guest places order with
// EventId → staff closes the event → assert attendance bonus credited.
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class EventManagementE2ETests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record IdResp(Guid Id);
    private sealed record PlaceResp(Guid OrderId);

    [Test]
    public async Task FullFlow_staff_creates_event_with_menu_then_guest_orders_then_close_credits_attendance()
    {
        // ─── Setup: company + the catalog scaffolding the diagram references. ───
        var company = await factory.SeedCompanyAsync();
        Guid outletId, productId, priceGroupId, productListId, userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var outlet = await db.Outlets.IgnoreQueryFilters().FirstAsync(x => x.CompanyId == company.Id);
            outletId = outlet.Id;
            var taxRate = TaxRate.Create("VAT 23%", "Standard", 23m, company.Id);
            db.TaxRates.Add(taxRate);
            var product = Product.Create("Acoustic Set Coffee", taxRate.Id, company.Id);
            db.Products.Add(product);
            productId = product.Id;
            var pg = PriceGroup.Create("Acoustic Night", company.Id);
            db.PriceGroups.Add(pg);
            priceGroupId = pg.Id;
            // Event price (cheaper than standard — typical event special).
            db.ProductPrices.Add(ProductPrice.Create(product.Id, pg.Id, 4.00m));
            var user = AppUser.Create($"guest-{Guid.NewGuid()}@test.local", "Eva", "Attendee", AccountType.Guest, null);
            db.Users.Add(user);
            userId = user.Id;
            await db.SaveChangesAsync();
        }

        using var staff = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions:
        [
            Permissions.MenusManage,
            Permissions.EventsManage,
            Permissions.OrdersManage
        ]);
        using var guest = factory.CreateClientAs(AccountType.Guest, userId: userId);

        // ─── Staff: create dedicated ProductList for the event. ───
        var listResp = await staff.PostAsJsonAsync("/api/product-lists", new { name = "Acoustic Night Menu" });
        productListId = (await listResp.Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await staff.PostAsJsonAsync($"/api/product-lists/{productListId}/items", new { productId }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ─── Staff: create the Event tied to that menu + price group. ───
        var eventResp = await staff.PostAsJsonAsync("/api/events", new
        {
            name = "Acoustic Night",
            description = "Live unplugged set",
            imageUrl = "https://cdn/acoustic.png",
            productListId,
            priceGroupId
        });
        var eventId = (await eventResp.Content.ReadFromJsonAsync<IdResp>())!.Id;

        // ─── Staff: add 2 days (the diagram's "Czy więcej dni?" loop). ───
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        (await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date = today }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await staff.PostAsJsonAsync($"/api/events/{eventId}/days", new { date = today.AddDays(1) }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // ─── Staff: publish (diagrammed end-state of the staff column). ───
        (await staff.PostAsync($"/api/events/{eventId}/publish", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ─── Guest: place order with EventId at the event price. ───
        var place = await guest.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            eventId,
            loyaltyPointsUsed = 0,
            // PriceGroupId pinned so PlaceOrder picks the cheaper event price (FIFO would
            // also work here since we only seeded one ProductPrice row, but being explicit
            // mirrors what the mobile app would do once it has the event context).
            lines = new[] { new { productId, quantity = 1, priceGroupId = (Guid?)priceGroupId } }
        });
        place.StatusCode.Should().Be(HttpStatusCode.OK);

        // ─── Staff: close the event → triggers attendance loyalty payout. ───
        (await staff.PostAsync($"/api/events/{eventId}/close", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ─── Assertion: attendance bonus credited to the guest. ───
        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var bonus = await db2.LoyaltyPointLogs.IgnoreQueryFilters()
            .Where(x => x.UserId == userId && x.Reason!.Contains("Attendance bonus"))
            .SumAsync(x => (int?)x.Points) ?? 0;
        bonus.Should().Be(CloseEvent.AttendanceBonusPoints);
    }
}
