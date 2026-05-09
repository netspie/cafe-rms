using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListUsersTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record UserItem(Guid Id, string Email, string FirstName, string LastName, IReadOnlyList<string> Roles);

    [Test]
    public async Task ListUsers_without_UsersManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
