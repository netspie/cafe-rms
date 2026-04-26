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
        var company = await factory.SeedCompanyAsync();
        var role = await factory.SeedRoleAsync("Cashier", company.Id);
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", companyId: company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsync($"/api/users/{user.Id}/roles/{role.Id}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AssignRole_idempotent_re_assignment_is_ok()
    {
        var company = await factory.SeedCompanyAsync();
        var role = await factory.SeedRoleAsync("Cashier", company.Id);
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", companyId: company.Id);
        await factory.AssignRoleAsync(user.Id, role.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsync($"/api/users/{user.Id}/roles/{role.Id}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AssignRole_user_not_in_company_returns_403()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        var role = await factory.SeedRoleAsync("Cashier", ownCompany.Id);
        var foreignUser = await factory.SeedUserAsync(AccountType.Staff, "foreign@x.local", "Pass1234!", companyId: otherCompany.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.RolesManage]);

        var response = await client.PostAsync($"/api/users/{foreignUser.Id}/roles/{role.Id}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task AssignRole_without_RolesManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var role = await factory.SeedRoleAsync("Cashier", company.Id);
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", companyId: company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PostAsync($"/api/users/{user.Id}/roles/{role.Id}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
