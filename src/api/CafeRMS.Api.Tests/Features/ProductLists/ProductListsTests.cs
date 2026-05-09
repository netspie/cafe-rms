using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ProductLists;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.ProductLists;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ProductListsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record ListDetail(Guid Id, string Name, IReadOnlyList<Guid> ProductIds, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record ListItem(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<ListItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.PostAsJsonAsync("/api/product-lists", new { name = "Breakfast Menu" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_duplicate_name_returns_409()
    {
        await SeedListAsync("Breakfast Menu");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.PostAsJsonAsync("/api/product-lists", new { name = "Breakfast Menu" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_without_MenusManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/product-lists", new { name = "Breakfast" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_list_with_product_ids()
    {
        var listId = await SeedListAsync("Breakfast Menu");
        var productId = await SeedProductAsync("Croissant");
        await SeedItemAsync(listId, productId);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.GetAsync($"/api/product-lists/{listId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ListDetail>();
        body!.ProductIds.Should().Equal(productId);
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var listId = await SeedListAsync("Breakfast");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.PutAsJsonAsync($"/api/product-lists/{listId}", new { name = "Brunch" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_hard_deletes_items()
    {
        var listId = await SeedListAsync("Breakfast");
        var productId = await SeedProductAsync("Croissant");
        await SeedItemAsync(listId, productId);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.DeleteAsync($"/api/product-lists/{listId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var leftover = await db.ProductListItems.IgnoreQueryFilters().CountAsync(x => x.ProductListId == listId);
        leftover.Should().Be(0);
    }

    [Test]
    public async Task AddItem_happy_path_returns_204()
    {
        var listId = await SeedListAsync("Breakfast");
        var productId = await SeedProductAsync("Croissant");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.PostAsJsonAsync($"/api/product-lists/{listId}/items", new { productId });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AddItem_is_idempotent()
    {
        var listId = await SeedListAsync("Breakfast");
        var productId = await SeedProductAsync("Croissant");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);
        await client.PostAsJsonAsync($"/api/product-lists/{listId}/items", new { productId });

        var response = await client.PostAsJsonAsync($"/api/product-lists/{listId}/items", new { productId });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AddItem_with_unknown_product_returns_404()
    {
        var listId = await SeedListAsync("Breakfast");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.PostAsJsonAsync($"/api/product-lists/{listId}/items", new { productId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task RemoveItem_returns_404_when_not_in_list()
    {
        var listId = await SeedListAsync("Breakfast");
        var productId = await SeedProductAsync("Croissant");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.MenusManage]);

        var response = await client.DeleteAsync($"/api/product-lists/{listId}/items/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedListAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var list = ProductList.Create(name);
        db.ProductLists.Add(list);
        await db.SaveChangesAsync();
        return list.Id;
    }

    private async Task<Guid> SeedProductAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = TaxRate.Create("VAT", "x", 23m);
        db.TaxRates.Add(taxRate);
        var product = Product.Create(name, taxRate.Id);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product.Id;
    }

    private async Task SeedItemAsync(Guid productListId, Guid productId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ProductListItems.Add(ProductListItem.Create(productListId, productId));
        await db.SaveChangesAsync();
    }
}
