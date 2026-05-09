using System.Net;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class AssignRoleTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task AssignRole_happy_path_returns_204()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsync($"/api/users/{user.Id}/roles/{role.Id}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AssignRole_idempotent_re_assignment_is_ok()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!");
        await factory.AssignRoleAsync(user.Id, role.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsync($"/api/users/{user.Id}/roles/{role.Id}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AssignRole_without_RolesManage_returns_403()
    {
        var role = await factory.SeedRoleAsync("Cashier");
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsync($"/api/users/{user.Id}/roles/{role.Id}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
