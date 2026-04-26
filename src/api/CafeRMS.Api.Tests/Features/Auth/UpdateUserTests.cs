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
        var company = await factory.SeedCompanyAsync();
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", companyId: company.Id, firstName: "Old", lastName: "Name");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.UsersManage]);

        var response = await client.PutAsJsonAsync($"/api/users/{user.Id}", new { firstName = "New", lastName = "Surname" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateUser_in_other_company_returns_403()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        var foreign = await factory.SeedUserAsync(AccountType.Staff, "foreign@x.local", "Pass1234!", companyId: otherCompany.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.UsersManage]);

        var response = await client.PutAsJsonAsync($"/api/users/{foreign.Id}", new { firstName = "X", lastName = "Y" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task UpdateUser_without_UsersManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", companyId: company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PutAsJsonAsync($"/api/users/{user.Id}", new { firstName = "X", lastName = "Y" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
