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
        var company = await factory.SeedCompanyAsync();
        var taxRateId = await SeedTaxRateAsync(company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId, barcode = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_duplicate_name_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        var taxRateId = await SeedTaxRateAsync(company.Id);
        await SeedProductAsync(company.Id, "Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_duplicate_barcode_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        var taxRateId = await SeedTaxRateAsync(company.Id);
        await SeedProductAsync(company.Id, "Espresso", taxRateId, barcode: "5901234567890");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Doppio", taxRateId, barcode = "5901234567890" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_with_unknown_taxRate_returns_404()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Add_without_ProductsManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var taxRateId = await SeedTaxRateAsync(company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PostAsJsonAsync("/api/products", new { name = "Espresso", taxRateId });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_product_with_empty_collections()
    {
        var company = await factory.SeedCompanyAsync();
        var taxRateId = await SeedTaxRateAsync(company.Id);
        var id = await SeedProductAsync(company.Id, "Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

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
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task List_filter_by_taxRateId()
    {
        var company = await factory.SeedCompanyAsync();
        var t23 = await SeedTaxRateAsync(company.Id, "VAT 23%", 23m);
        var t8 = await SeedTaxRateAsync(company.Id, "VAT 8%", 8m);
        await SeedProductAsync(company.Id, "Espresso", t23);
        await SeedProductAsync(company.Id, "Croissant", t8);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync($"/api/products?taxRateId={t8}");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("Croissant");
    }

    [Test]
    public async Task List_does_not_include_other_companies_items()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        var ownTax = await SeedTaxRateAsync(ownCompany.Id);
        var otherTax = await SeedTaxRateAsync(otherCompany.Id);
        await SeedProductAsync(ownCompany.Id, "OurEspresso", ownTax);
        await SeedProductAsync(otherCompany.Id, "TheirEspresso", otherTax);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync("/api/products");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurEspresso");
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var taxRateId = await SeedTaxRateAsync(company.Id);
        var id = await SeedProductAsync(company.Id, "Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PutAsJsonAsync($"/api/products/{id}", new { name = "Doppio Espresso", taxRateId });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var taxRateId = await SeedTaxRateAsync(company.Id);
        var id = await SeedProductAsync(company.Id, "Espresso", taxRateId);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedTaxRateAsync(Guid companyId, string name = "VAT 23%", decimal rate = 23m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = TaxRate.Create(name, "x", rate, companyId);
        db.TaxRates.Add(taxRate);
        await db.SaveChangesAsync();
        return taxRate.Id;
    }

    private async Task<Guid> SeedProductAsync(Guid companyId, string name, Guid taxRateId, string? barcode = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var product = Product.Create(name, taxRateId, companyId, barcode: barcode);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product.Id;
    }
}
