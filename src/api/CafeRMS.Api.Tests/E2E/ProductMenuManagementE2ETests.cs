using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.E2E;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ProductMenuManagementE2ETests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record IdResp(Guid Id);
    private sealed record ProductDetail(
        Guid Id, string Name, string? Description, string? Barcode, Guid TaxRateId,
        IReadOnlyList<Guid> TagIds, IReadOnlyList<Guid> AllergenIds, IReadOnlyList<Guid> ModifierGroupIds,
        IReadOnlyList<ImageInfo> Images, IReadOnlyList<PriceInfo> Prices,
        DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record ImageInfo(Guid Id, string Url);
    private sealed record PriceInfo(Guid PriceGroupId, decimal Net);
    private sealed record ProductListDetail(
        Guid Id, string Name, IReadOnlyList<Guid> ProductIds,
        DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    [Test]
    public async Task FullFlow_staff_sets_up_complete_product_and_menu()
    {
        using var staff = factory.CreateClientAs(AccountType.Staff, permissions:
        [
            Permissions.TaxRatesManage,
            Permissions.PricingManage,
            Permissions.ProductsManage,
            Permissions.ModifiersManage,
            Permissions.MenusManage
        ]);

        var taxRateId = await CreateAsync(staff, "/api/tax-rates", new { name = "VAT 23%", description = "Standard rate", rate = 23m });
        var priceGroupId = await CreateAsync(staff, "/api/price-groups", new { name = "Standard" });
        var tagId = await CreateAsync(staff, "/api/tags", new { name = "Vegan", imageUrl = (string?)null });
        var allergenId = await CreateAsync(staff, "/api/allergens", new { name = "Peanuts" });
        var modifierGroupId = await CreateAsync(staff, "/api/modifier-groups", new { name = "Milk type" });

        var productId = await CreateAsync(staff, "/api/products", new
        {
            name = "Espresso",
            description = "Single shot",
            barcode = "5901234567890",
            taxRateId
        });

        (await staff.PutAsJsonAsync($"/api/products/{productId}/prices/{priceGroupId}", new { net = 5.00m }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await staff.PostAsync($"/api/products/{productId}/tags/{tagId}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await staff.PostAsync($"/api/products/{productId}/allergens/{allergenId}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var imgResp = await staff.PostAsJsonAsync($"/api/products/{productId}/images", new { url = "https://cdn/espresso.png" });
        imgResp.StatusCode.Should().Be(HttpStatusCode.OK);

        (await staff.PostAsync($"/api/products/{productId}/modifier-groups/{modifierGroupId}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listId = await CreateAsync(staff, "/api/product-lists", new { name = "Breakfast Menu" });
        (await staff.PostAsJsonAsync($"/api/product-lists/{listId}/items", new { productId }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var productResp = await staff.GetAsync($"/api/products/{productId}");
        var product = await productResp.Content.ReadFromJsonAsync<ProductDetail>();
        product!.Name.Should().Be("Espresso");
        product.TagIds.Should().Equal(tagId);
        product.AllergenIds.Should().Equal(allergenId);
        product.ModifierGroupIds.Should().Equal(modifierGroupId);
        product.Images.Should().HaveCount(1);
        product.Prices.Should().ContainSingle(p => p.PriceGroupId == priceGroupId && p.Net == 5.00m);

        var listResp = await staff.GetAsync($"/api/product-lists/{listId}");
        var list = await listResp.Content.ReadFromJsonAsync<ProductListDetail>();
        list!.ProductIds.Should().Equal(productId);
    }

    private static async Task<Guid> CreateAsync(HttpClient client, string url, object body)
    {
        var resp = await client.PostAsJsonAsync(url, body);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await resp.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }
}
