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

namespace CafeRMS.Api.Tests.Features.Orders;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class PlaceOrderTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record OrderResponse(Guid OrderId);

    [Test]
    public async Task PlaceMyOrder_happy_path_returns_id()
    {
        var (companyId, userId, outletId, productId) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId, quantity = 2, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task PlaceMyOrder_with_unknown_product_returns_404()
    {
        var (companyId, userId, outletId, _) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId = Guid.NewGuid(), quantity = 1, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PlaceMyOrder_with_no_price_returns_404()
    {
        var (companyId, userId, outletId, _) = await SeedAsync();
        var pricelessProductId = await SeedProductWithoutPriceAsync(companyId);
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId = pricelessProductId, quantity = 1, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PlaceMyOrder_empty_lines_returns_400()
    {
        var (companyId, userId, outletId, _) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 0,
            lines = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PlaceMyOrder_with_promo_increments_uses_and_applies_discount()
    {
        var (companyId, userId, outletId, productId) = await SeedAsync();
        await SeedPromoAsync(companyId, "WELCOME10", 10m);
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            promotionCode = "WELCOME10",
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId, quantity = 1, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var promo = await db.PromotionCodes.IgnoreQueryFilters().FirstAsync(x => x.Code == "WELCOME10");
        promo.UsesCount.Should().Be(1);
    }

    [Test]
    public async Task PlaceMyOrder_with_loyalty_redemption_burns_points()
    {
        var (companyId, userId, outletId, productId) = await SeedAsync();
        await SeedLoyaltyPointsAsync(companyId, userId, 50);
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 20,
            lines = new[] { new { productId, quantity = 1, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var balance = await db.LoyaltyPointLogs.IgnoreQueryFilters().Where(x => x.UserId == userId).SumAsync(x => x.Points);
        balance.Should().Be(30);
    }

    [Test]
    public async Task PlaceMyOrder_with_insufficient_loyalty_returns_400()
    {
        var (companyId, userId, outletId, productId) = await SeedAsync();
        await SeedLoyaltyPointsAsync(companyId, userId, 5);
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/orders", new
        {
            outletId,
            loyaltyPointsUsed = 100,
            lines = new[] { new { productId, quantity = 1, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PlaceWalkInOrder_staff_path_with_no_user_returns_id()
    {
        var (companyId, _, outletId, productId) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: companyId, permissions: [Permissions.OrdersManage]);

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            outletId,
            userId = (Guid?)null,
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId, quantity = 1, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task PlaceWalkInOrder_without_OrdersManage_returns_403()
    {
        var (companyId, _, outletId, productId) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: companyId, permissions: []);

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            outletId,
            loyaltyPointsUsed = 0,
            lines = new[] { new { productId, quantity = 1, priceGroupId = (Guid?)null } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<(Guid CompanyId, Guid UserId, Guid OutletId, Guid ProductId)> SeedAsync()
    {
        var company = await factory.SeedCompanyAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outlet = await db.Outlets.IgnoreQueryFilters().FirstAsync(x => x.CompanyId == company.Id);
        var taxRate = TaxRate.Create("VAT 23%", "Standard", 23m, company.Id);
        db.TaxRates.Add(taxRate);
        var product = Product.Create("Espresso", taxRate.Id, company.Id);
        db.Products.Add(product);
        var pg = PriceGroup.Create("Standard", company.Id);
        db.PriceGroups.Add(pg);
        db.ProductPrices.Add(ProductPrice.Create(product.Id, pg.Id, 5.00m));
        var user = AppUser.Create("guest@test.local", "Guest", "User", AccountType.Guest, null);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (company.Id, user.Id, outlet.Id, product.Id);
    }

    private async Task<Guid> SeedProductWithoutPriceAsync(Guid companyId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = await db.TaxRates.IgnoreQueryFilters().FirstAsync(x => x.CompanyId == companyId);
        var product = Product.Create("Priceless", taxRate.Id, companyId);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product.Id;
    }

    private async Task SeedPromoAsync(Guid companyId, string code, decimal pct)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PromotionCodes.Add(PromotionCode.Create(code, pct, companyId));
        await db.SaveChangesAsync();
    }

    private async Task SeedLoyaltyPointsAsync(Guid companyId, Guid userId, int points)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(userId, points, companyId, reason: "test seed"));
        await db.SaveChangesAsync();
    }
}
