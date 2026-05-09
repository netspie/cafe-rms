using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UpdateUserTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UpdateUser_happy_path_returns_204()
    {
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", firstName: "Old", lastName: "Name");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.UsersManage]);

        var response = await client.PutAsJsonAsync($"/api/users/{user.Id}", new { firstName = "New", lastName = "Surname" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateUser_without_UsersManage_returns_403()
    {
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PutAsJsonAsync($"/api/users/{user.Id}", new { firstName = "X", lastName = "Y" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
