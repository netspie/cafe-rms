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
    public async Task ListUsers_returns_staff_in_current_company_only()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await factory.SeedUserAsync(AccountType.Staff, "ours@x.local", "Pass1234!", companyId: ownCompany.Id, firstName: "A", lastName: "Ours");
        await factory.SeedUserAsync(AccountType.Staff, "theirs@x.local", "Pass1234!", companyId: otherCompany.Id, firstName: "B", lastName: "Theirs");
        await factory.SeedUserAsync(AccountType.Guest, "guest@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.UsersManage]);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<UserItem>>();
        body.Should().NotBeNull().And.HaveCount(1);
        body![0].Email.Should().Be("ours@x.local");
    }

    [Test]
    public async Task ListUsers_without_UsersManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
