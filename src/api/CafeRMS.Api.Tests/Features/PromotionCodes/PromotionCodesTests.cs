using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PromotionCodes;
using CafeRMS.Api.Features.PromotionCodes.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.PromotionCodes;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class PromotionCodesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record PromoDetail(
        Guid Id, string Code, decimal DiscountPercentage,
        DateTimeOffset? ValidFrom, DateTimeOffset? ValidUntil, int? MaxUses, int UsesCount,
        DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record PromoItem(
        Guid Id, string Code, decimal DiscountPercentage,
        DateTimeOffset? ValidFrom, DateTimeOffset? ValidUntil, int? MaxUses, int UsesCount,
        DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<PromoItem> Items, int Page, int PageSize, int Total);
    private sealed record ValidateResponse(bool Valid, decimal? DiscountPercentage, string? Reason);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PromotionsManage]);

        var response = await client.PostAsJsonAsync("/api/promotion-codes", new { code = "WELCOME10", discountPercentage = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_duplicate_code_returns_409()
    {
        await SeedPromoAsync("WELCOME10");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PromotionsManage]);

        var response = await client.PostAsJsonAsync("/api/promotion-codes", new { code = "WELCOME10", discountPercentage = 5m });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_with_validFrom_after_validUntil_returns_400()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PromotionsManage]);

        var response = await client.PostAsJsonAsync("/api/promotion-codes", new
        {
            code = "BACKWARDS",
            discountPercentage = 10m,
            validFrom = DateTimeOffset.UtcNow.AddDays(5),
            validUntil = DateTimeOffset.UtcNow.AddDays(1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Add_without_PromotionsManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/promotion-codes", new { code = "X", discountPercentage = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_promo()
    {
        var id = await SeedPromoAsync("WELCOME10");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PromotionsManage]);

        var response = await client.GetAsync($"/api/promotion-codes/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PromoDetail>();
        body!.Code.Should().Be("WELCOME10");
        body.UsesCount.Should().Be(0);
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var id = await SeedPromoAsync("WELCOME10");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PromotionsManage]);

        var response = await client.PutAsJsonAsync($"/api/promotion-codes/{id}", new
        {
            code = "WELCOME15",
            discountPercentage = 15m,
            maxUses = 100
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_happy_path_returns_204()
    {
        var id = await SeedPromoAsync("WELCOME10");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PromotionsManage]);

        var response = await client.DeleteAsync($"/api/promotion-codes/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Validate_valid_code_returns_200_with_discount()
    {
        await SeedPromoAsync("WELCOME10", discountPercentage: 10m);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/promotion-codes/validate", new { code = "WELCOME10" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ValidateResponse>();
        body!.Valid.Should().BeTrue();
        body.DiscountPercentage.Should().Be(10m);
    }

    [Test]
    public async Task Validate_unknown_code_returns_invalid_with_reason()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/promotion-codes/validate", new { code = "DOESNOTEXIST" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ValidateResponse>();
        body!.Valid.Should().BeFalse();
        body.Reason.Should().Be(ValidatePromotionCode.ReasonNotFound);
    }

    [Test]
    public async Task Validate_expired_code_returns_invalid_with_expired_reason()
    {
        await SeedPromoAsync(
            "EXPIRED", discountPercentage: 10m,
            validUntil: DateTimeOffset.UtcNow.AddDays(-1));
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/promotion-codes/validate", new { code = "EXPIRED" });

        var body = await response.Content.ReadFromJsonAsync<ValidateResponse>();
        body!.Valid.Should().BeFalse();
        body.Reason.Should().Be(ValidatePromotionCode.ReasonExpired);
    }

    [Test]
    public async Task Validate_max_uses_reached_returns_invalid_with_reason()
    {
        await SeedPromoAsync(
            "MAXED", discountPercentage: 10m,
            maxUses: 1, usesCount: 1);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/promotion-codes/validate", new { code = "MAXED" });

        var body = await response.Content.ReadFromJsonAsync<ValidateResponse>();
        body!.Valid.Should().BeFalse();
        body.Reason.Should().Be(ValidatePromotionCode.ReasonMaxUsesReached);
    }

    private async Task<Guid> SeedPromoAsync(
        string code,
        decimal discountPercentage = 10m,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validUntil = null,
        int? maxUses = null,
        int usesCount = 0)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var promo = PromotionCode.Create(code, discountPercentage, validFrom, validUntil, maxUses);
        if (usesCount > 0)
        {
            // UsesCount is private set via Update only updating settable fields; for tests
            // we set it directly through the EF property bag.
            db.Entry(promo).Property("UsesCount").CurrentValue = usesCount;
        }
        db.PromotionCodes.Add(promo);
        await db.SaveChangesAsync();
        return promo.Id;
    }
}
