using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListRolesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record RoleItem(Guid Id, string Name, IReadOnlyList<string> Permissions);

    [Test]
    public async Task ListRoles_returns_roles_in_current_company()
    {
        await factory.SeedRoleAsync("Cashier", permissions: [Permissions.OrdersManage]);
        await factory.SeedRoleAsync("Manager", permissions: [Permissions.OrdersManage, Permissions.UsersManage]);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.GetAsync("/api/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<RoleItem>>();
        body.Should().NotBeNull().And.HaveCount(2);
        body!.Single(x => x.Name == "Cashier").Permissions.Should().BeEquivalentTo(new[] { Permissions.OrdersManage });
    }

    [Test]
    public async Task ListRoles_without_RolesManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.GetAsync("/api/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
