using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Allergens;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.Tags;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Products;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ProductsSubResourcesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record ProductDetail(
        Guid Id, string Name, string? Description, string? Barcode, Guid TaxRateId,
        IReadOnlyList<Guid> TagIds, IReadOnlyList<Guid> AllergenIds, IReadOnlyList<Guid> ModifierGroupIds,
        IReadOnlyList<ImageInfo> Images, IReadOnlyList<PriceInfo> Prices,
        DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record ImageInfo(Guid Id, string Url);
    private sealed record PriceInfo(Guid PriceGroupId, decimal Net);
    private sealed record ImageResponse(Guid Id);

    [Test]
    public async Task AttachTag_happy_path_then_visible_in_GetById()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        var tagId = await SeedTagAsync("Vegan");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var attach = await client.PostAsync($"/api/products/{productId}/tags/{tagId}", null);
        attach.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await client.GetAsync($"/api/products/{productId}");
        var body = await get.Content.ReadFromJsonAsync<ProductDetail>();
        body!.TagIds.Should().Equal(tagId);
    }

    [Test]
    public async Task AttachTag_is_idempotent()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        var tagId = await SeedTagAsync("Vegan");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);
        await client.PostAsync($"/api/products/{productId}/tags/{tagId}", null);

        var response = await client.PostAsync($"/api/products/{productId}/tags/{tagId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DetachTag_returns_404_when_not_attached()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        var tagId = await SeedTagAsync("Vegan");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/products/{productId}/tags/{tagId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AttachAllergen_then_DetachAllergen_round_trip()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        var allergenId = await SeedAllergenAsync("Peanuts");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var attach = await client.PostAsync($"/api/products/{productId}/allergens/{allergenId}", null);
        attach.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var detach = await client.DeleteAsync($"/api/products/{productId}/allergens/{allergenId}");
        detach.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AttachModifierGroup_with_unknown_group_returns_404()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsync($"/api/products/{productId}/modifier-groups/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AttachModifierGroup_happy_path()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        var groupId = await SeedModifierGroupAsync("Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var attach = await client.PostAsync($"/api/products/{productId}/modifier-groups/{groupId}", null);
        attach.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await client.GetAsync($"/api/products/{productId}");
        var body = await get.Content.ReadFromJsonAsync<ProductDetail>();
        body!.ModifierGroupIds.Should().Equal(groupId);
    }

    [Test]
    public async Task AddImage_then_RemoveImage_round_trip()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var add = await client.PostAsJsonAsync($"/api/products/{productId}/images", new { url = "https://cdn/espresso.png" });
        add.StatusCode.Should().Be(HttpStatusCode.OK);
        var addBody = await add.Content.ReadFromJsonAsync<ImageResponse>();

        var get = await client.GetAsync($"/api/products/{productId}");
        var detail = await get.Content.ReadFromJsonAsync<ProductDetail>();
        detail!.Images.Should().HaveCount(1);
        detail.Images[0].Url.Should().Be("https://cdn/espresso.png");

        var remove = await client.DeleteAsync($"/api/products/{productId}/images/{addBody!.Id}");
        remove.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task RemoveImage_returns_404_when_not_on_product()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/products/{productId}/images/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SetPrice_upsert_overwrites_existing()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        var pgId = await SeedPriceGroupAsync("Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        await client.PutAsJsonAsync($"/api/products/{productId}/prices/{pgId}", new { net = 5.00m });
        await client.PutAsJsonAsync($"/api/products/{productId}/prices/{pgId}", new { net = 6.50m });

        var get = await client.GetAsync($"/api/products/{productId}");
        var body = await get.Content.ReadFromJsonAsync<ProductDetail>();
        body!.Prices.Should().HaveCount(1);
        body.Prices[0].Net.Should().Be(6.50m);
    }

    [Test]
    public async Task SetPrice_with_unknown_priceGroup_returns_404()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PutAsJsonAsync($"/api/products/{productId}/prices/{Guid.NewGuid()}", new { net = 5m });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteProduct_cleans_up_all_join_rows()
    {
        var (productId, _) = await SeedProductWithDepsAsync();
        var tagId = await SeedTagAsync("Vegan");
        var allergenId = await SeedAllergenAsync("Peanuts");
        var groupId = await SeedModifierGroupAsync("Milk type");
        var pgId = await SeedPriceGroupAsync("Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);
        await client.PostAsync($"/api/products/{productId}/tags/{tagId}", null);
        await client.PostAsync($"/api/products/{productId}/allergens/{allergenId}", null);
        await client.PostAsync($"/api/products/{productId}/modifier-groups/{groupId}", null);
        await client.PostAsJsonAsync($"/api/products/{productId}/images", new { url = "https://cdn/x.png" });
        await client.PutAsJsonAsync($"/api/products/{productId}/prices/{pgId}", new { net = 5m });

        var delete = await client.DeleteAsync($"/api/products/{productId}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.ProductTags.IgnoreQueryFilters().CountAsync(x => x.ProductId == productId)).Should().Be(0);
        (await db.ProductAllergens.IgnoreQueryFilters().CountAsync(x => x.ProductId == productId)).Should().Be(0);
        (await db.ProductModifierGroups.IgnoreQueryFilters().CountAsync(x => x.ProductId == productId)).Should().Be(0);
        (await db.ProductImages.IgnoreQueryFilters().CountAsync(x => x.ProductId == productId)).Should().Be(0);
        (await db.ProductPrices.IgnoreQueryFilters().CountAsync(x => x.ProductId == productId)).Should().Be(0);
    }

    private async Task<(Guid ProductId, Guid TaxRateId)> SeedProductWithDepsAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = TaxRate.Create("VAT 23%", "x", 23m);
        db.TaxRates.Add(taxRate);
        var product = Product.Create("Espresso", taxRate.Id);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return (product.Id, taxRate.Id);
    }

    private async Task<Guid> SeedTagAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tag = Tag.Create(name);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return tag.Id;
    }

    private async Task<Guid> SeedAllergenAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var allergen = Allergen.Create(name);
        db.Allergens.Add(allergen);
        await db.SaveChangesAsync();
        return allergen.Id;
    }

    private async Task<Guid> SeedModifierGroupAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var group = ModifierGroup.Create(name);
        db.ModifierGroups.Add(group);
        await db.SaveChangesAsync();
        return group.Id;
    }

    private async Task<Guid> SeedPriceGroupAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pg = PriceGroup.Create(name);
        db.PriceGroups.Add(pg);
        await db.SaveChangesAsync();
        return pg.Id;
    }
}
