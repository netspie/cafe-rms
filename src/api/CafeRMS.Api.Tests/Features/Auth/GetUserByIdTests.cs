using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class GetUserByIdTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record UserDetail(Guid Id, string Email, string FirstName, string LastName, IReadOnlyList<string> Roles);

    [Test]
    public async Task GetById_happy_path_returns_user_with_roles()
    {
        var company = await factory.SeedCompanyAsync();
        var role = await factory.SeedRoleAsync("Cashier", company.Id);
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", companyId: company.Id);
        await factory.AssignRoleAsync(user.Id, role.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.UsersManage]);

        var response = await client.GetAsync($"/api/users/{user.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDetail>();
        body!.Email.Should().Be("staff@x.local");
        body.Roles.Should().ContainSingle().Which.Should().Be("Cashier");
    }

    [Test]
    public async Task GetById_user_in_other_company_returns_403()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        var foreign = await factory.SeedUserAsync(AccountType.Staff, "foreign@x.local", "Pass1234!", companyId: otherCompany.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.UsersManage]);

        var response = await client.GetAsync($"/api/users/{foreign.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_404_for_nonexistent()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.UsersManage]);

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetById_without_UsersManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var user = await factory.SeedUserAsync(AccountType.Staff, "staff@x.local", "Pass1234!", companyId: company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.GetAsync($"/api/users/{user.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
