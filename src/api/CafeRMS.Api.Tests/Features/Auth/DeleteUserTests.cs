using System.Net;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteUserTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeleteUser_happy_path_returns_204()
    {
        var target = await factory.SeedUserAsync(AccountType.Staff, "target@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, userId: Guid.NewGuid(), permissions: [Permissions.UsersManage]);

        var response = await client.DeleteAsync($"/api/users/{target.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteUser_self_delete_returns_409()
    {
        var self = await factory.SeedUserAsync(AccountType.Staff, "me@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, userId: self.Id, permissions: [Permissions.UsersManage]);

        var response = await client.DeleteAsync($"/api/users/{self.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DeleteUser_last_Owner_returns_409()
    {
        var ownerRole = await factory.SeedRoleAsync(SystemRoles.Owner);
        var lastOwner = await factory.SeedUserAsync(AccountType.Staff, "owner@x.local", "Pass1234!");
        await factory.AssignRoleAsync(lastOwner.Id, ownerRole.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, userId: Guid.NewGuid(), permissions: [Permissions.UsersManage]);

        var response = await client.DeleteAsync($"/api/users/{lastOwner.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DeleteUser_without_UsersManage_returns_403()
    {
        var target = await factory.SeedUserAsync(AccountType.Staff, "target@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, userId: Guid.NewGuid(), permissions: []);

        var response = await client.DeleteAsync($"/api/users/{target.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
