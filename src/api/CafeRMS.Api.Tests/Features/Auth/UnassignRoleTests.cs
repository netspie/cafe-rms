using System.Net;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UnassignRoleTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UnassignRole_happy_path_returns_204()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!");
        await factory.AssignRoleAsync(user.Id, role.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.DeleteAsync($"/api/users/{user.Id}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UnassignRole_last_Owner_removal_returns_409()
    {
        var ownerRole = await factory.SeedRoleAsync(SystemRoles.Owner);
        var owner = await factory.SeedUserAsync(AccountType.Staff, "owner@x.local", "Pass1234!");
        await factory.AssignRoleAsync(owner.Id, ownerRole.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.DeleteAsync($"/api/users/{owner.Id}/roles/{ownerRole.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task UnassignRole_when_user_does_not_have_role_returns_404()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.DeleteAsync($"/api/users/{user.Id}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UnassignRole_without_RolesManage_returns_403()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!");
        await factory.AssignRoleAsync(user.Id, role.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.DeleteAsync($"/api/users/{user.Id}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
