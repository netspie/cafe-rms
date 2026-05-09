using System.Net;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteRoleTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeleteRole_happy_path_returns_204()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.DeleteAsync($"/api/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteRole_targeting_Owner_returns_403()
    {
        var ownerRole = await factory.SeedRoleAsync(SystemRoles.Owner);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.DeleteAsync($"/api/roles/{ownerRole.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteRole_returns_404_for_nonexistent_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.DeleteAsync($"/api/roles/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteRole_without_RolesManage_returns_403()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.DeleteAsync($"/api/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
