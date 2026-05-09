using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Tables;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class AddTableTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task AddTable_happy_path_returns_id()
    {
        await factory.SeedOutletAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TablesManage]);

        var response = await client.PostAsJsonAsync("/api/tables", new { name = "Table 1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task AddTable_duplicate_name_returns_409()
    {
        await factory.SeedOutletAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TablesManage]);
        await client.PostAsJsonAsync("/api/tables", new { name = "Table 1" });

        var response = await client.PostAsJsonAsync("/api/tables", new { name = "Table 1" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddTable_without_TablesManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/tables", new { name = "Table 1" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
