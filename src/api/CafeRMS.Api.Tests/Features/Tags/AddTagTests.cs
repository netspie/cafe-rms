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
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PostAsJsonAsync("/api/tags", new { name = "Vegan", imageUrl = "https://cdn/vegan.png" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task AddTag_duplicate_name_returns_409()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);
        await client.PostAsJsonAsync("/api/tags", new { name = "Vegan" });

        var response = await client.PostAsJsonAsync("/api/tags", new { name = "Vegan" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddTag_without_ProductsManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/tags", new { name = "Vegan" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
