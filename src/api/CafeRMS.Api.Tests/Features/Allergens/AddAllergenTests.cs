using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Allergens;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class AddAllergenTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task AddAllergen_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/allergens", new { name = "Peanuts" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task AddAllergen_duplicate_name_returns_409()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);
        await client.PostAsJsonAsync("/api/allergens", new { name = "Peanuts" });

        var response = await client.PostAsJsonAsync("/api/allergens", new { name = "Peanuts" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddAllergen_without_ProductsManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/allergens", new { name = "Peanuts" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
