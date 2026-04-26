using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Tags;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class AddTagTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task AddTag_happy_path_returns_id()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/tags", new { name = "Vegan", imageUrl = "https://cdn/vegan.png" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task AddTag_duplicate_name_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);
        await client.PostAsJsonAsync("/api/tags", new { name = "Vegan" });

        var response = await client.PostAsJsonAsync("/api/tags", new { name = "Vegan" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddTag_without_ProductsManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PostAsJsonAsync("/api/tags", new { name = "Vegan" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
