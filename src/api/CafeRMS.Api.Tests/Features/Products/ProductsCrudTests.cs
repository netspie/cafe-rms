using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Products;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ProductsCrudTests : IDisposable
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
    private sealed record ProductItem(Guid Id, string Name, string? Barcode, Guid TaxRateId, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<ProductItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        var taxRateId = await SeedTaxRateAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId, barcode = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_duplicate_name_returns_409()
    {
        var taxRateId = await SeedTaxRateAsync();
        await SeedProductAsync("Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_duplicate_barcode_returns_409()
    {
        var taxRateId = await SeedTaxRateAsync();
        await SeedProductAsync("Espresso", taxRateId, barcode: "5901234567890");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Doppio", taxRateId, barcode = "5901234567890" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_with_unknown_taxRate_returns_404()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Add_without_ProductsManage_returns_403()
    {
        var taxRateId = await SeedTaxRateAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_product_with_empty_collections()
    {
        var taxRateId = await SeedTaxRateAsync();
        var id = await SeedProductAsync("Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductDetail>();
        body!.Name.Should().Be("Espresso");
        body.TagIds.Should().BeEmpty();
        body.Prices.Should().BeEmpty();
    }

    [Test]
    public async Task GetById_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task List_filter_by_taxRateId()
    {
        var t23 = await SeedTaxRateAsync("VAT 23%", 23m);
        var t8 = await SeedTaxRateAsync("VAT 8%", 8m);
        await SeedProductAsync("Espresso", t23);
        await SeedProductAsync("Croissant", t8);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync($"/api/products?taxRateId={t8}");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("Croissant");
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var taxRateId = await SeedTaxRateAsync();
        var id = await SeedProductAsync("Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PutAsJsonAsync($"/api/products/{id}", new { name = "Doppio Espresso", taxRateId });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_happy_path_returns_204()
    {
        var taxRateId = await SeedTaxRateAsync();
        var id = await SeedProductAsync("Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedTaxRateAsync(string name = "VAT 23%", decimal rate = 23m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = TaxRate.Create(name, "x", rate);
        db.TaxRates.Add(taxRate);
        await db.SaveChangesAsync();
        return taxRate.Id;
    }

    private async Task<Guid> SeedProductAsync(string name, Guid taxRateId, string? barcode = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var product = Product.Create(name, taxRateId, barcode: barcode);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product.Id;
    }
}
