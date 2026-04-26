using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UpdateRoleTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UpdateRole_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var role = await factory.SeedRoleAsync("Cashier", company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.RolesManage]);

        var response = await client.PutAsJsonAsync($"/api/roles/{role.Id}", new
        {
            name = "Senior Cashier",
            permissions = new[] { Permissions.OrdersManage }
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateRole_targeting_Owner_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var ownerRole = await factory.SeedRoleAsync(SystemRoles.Owner, company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.RolesManage]);

        var response = await client.PutAsJsonAsync($"/api/roles/{ownerRole.Id}", new
        {
            name = "Renamed Owner",
            permissions = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task UpdateRole_renaming_to_Owner_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var role = await factory.SeedRoleAsync("Cashier", company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.RolesManage]);

        var response = await client.PutAsJsonAsync($"/api/roles/{role.Id}", new
        {
            name = "Owner",
            permissions = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task UpdateRole_without_RolesManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var role = await factory.SeedRoleAsync("Cashier", company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PutAsJsonAsync($"/api/roles/{role.Id}", new { name = "X", permissions = Array.Empty<string>() });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
