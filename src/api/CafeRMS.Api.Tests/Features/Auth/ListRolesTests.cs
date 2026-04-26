using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListRolesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record RoleItem(Guid Id, string Name, IReadOnlyList<string> Permissions);

    [Test]
    public async Task ListRoles_returns_roles_in_current_company()
    {
        var company = await factory.SeedCompanyAsync();
        await factory.SeedRoleAsync("Cashier", company.Id, permissions: [Permissions.OrdersManage]);
        await factory.SeedRoleAsync("Manager", company.Id, permissions: [Permissions.OrdersManage, Permissions.UsersManage]);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.RolesManage]);

        var response = await client.GetAsync("/api/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<RoleItem>>();
        body.Should().NotBeNull().And.HaveCount(2);
        body!.Single(x => x.Name == "Cashier").Permissions.Should().BeEquivalentTo(new[] { Permissions.OrdersManage });
    }

    [Test]
    public async Task ListRoles_does_not_include_other_companies_roles()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await factory.SeedRoleAsync("OurCashier", ownCompany.Id);
        await factory.SeedRoleAsync("TheirCashier", otherCompany.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.RolesManage]);

        var response = await client.GetAsync("/api/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<RoleItem>>();
        body!.Select(x => x.Name).Should().BeEquivalentTo(new[] { "OurCashier" });
    }

    [Test]
    public async Task ListRoles_without_RolesManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.GetAsync("/api/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
