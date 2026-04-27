using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Favorites;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class FavoritesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record FavoriteItem(Guid ProductId, string ProductName, string? Description);

    [Test]
    public async Task Add_happy_path_returns_204()
    {
        var (userId, productId) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/favorites", new { productId });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Add_is_idempotent()
    {
        var (userId, productId) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);
        await client.PostAsJsonAsync("/api/my/favorites", new { productId });

        var response = await client.PostAsJsonAsync("/api/my/favorites", new { productId });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Add_unknown_product_returns_404()
    {
        var (userId, _) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.PostAsJsonAsync("/api/my/favorites", new { productId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task List_returns_only_own_favorites()
    {
        var (userA, productId) = await SeedAsync();
        var userB = await SeedExtraUserAsync();
        using var clientA = factory.CreateClientAs(AccountType.Guest, userId: userA);
        using var clientB = factory.CreateClientAs(AccountType.Guest, userId: userB);
        await clientA.PostAsJsonAsync("/api/my/favorites", new { productId });

        var response = await clientB.GetAsync("/api/my/favorites");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<FavoriteItem>>();
        body.Should().BeEmpty();
    }

    [Test]
    public async Task Remove_returns_404_when_not_favorited()
    {
        var (userId, productId) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.DeleteAsync($"/api/my/favorites/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Add_then_Remove_round_trip()
    {
        var (userId, productId) = await SeedAsync();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);
        await client.PostAsJsonAsync("/api/my/favorites", new { productId });

        var remove = await client.DeleteAsync($"/api/my/favorites/{productId}");
        remove.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await client.GetAsync("/api/my/favorites");
        var body = await list.Content.ReadFromJsonAsync<List<FavoriteItem>>();
        body.Should().BeEmpty();
    }

    private async Task<(Guid UserId, Guid ProductId)> SeedAsync()
    {
        var company = await factory.SeedCompanyAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = TaxRate.Create("VAT 23%", "x", 23m, company.Id);
        db.TaxRates.Add(taxRate);
        var product = Product.Create("Espresso", taxRate.Id, company.Id);
        db.Products.Add(product);
        var user = AppUser.Create($"guest-{Guid.NewGuid()}@test.local", "Guest", "User", AccountType.Guest, null);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, product.Id);
    }

    private async Task<Guid> SeedExtraUserAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = AppUser.Create($"guest-{Guid.NewGuid()}@test.local", "B", "User", AccountType.Guest, null);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }
}
