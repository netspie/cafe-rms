using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.PriceGroups;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class AddPriceGroupTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task AddPriceGroup_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PricingManage]);

        var response = await client.PostAsJsonAsync("/api/price-groups", new { name = "Standard" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task AddPriceGroup_duplicate_name_returns_409()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PricingManage]);
        await client.PostAsJsonAsync("/api/price-groups", new { name = "Standard" });

        var response = await client.PostAsJsonAsync("/api/price-groups", new { name = "Standard" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddPriceGroup_without_PricingManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/price-groups", new { name = "Standard" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
