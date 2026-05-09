using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class CreateRoleTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task CreateRole_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsJsonAsync("/api/roles", new
        {
            name = "Cashier",
            permissions = new[] { Permissions.OrdersManage, Permissions.OrdersView }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task CreateRole_named_Owner_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsJsonAsync("/api/roles", new { name = "Owner", permissions = Array.Empty<string>() });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task CreateRole_duplicate_name_returns_409()
    {
        await factory.SeedRoleAsync("Cashier");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsJsonAsync("/api/roles", new { name = "Cashier", permissions = Array.Empty<string>() });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task CreateRole_without_RolesManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/roles", new { name = "Cashier", permissions = Array.Empty<string>() });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
