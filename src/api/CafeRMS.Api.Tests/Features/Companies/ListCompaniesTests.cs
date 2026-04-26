using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Companies;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListCompaniesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record CompanyItem(Guid Id, string LegalName, string TaxId, string BillingEmail, bool IsPublic, DateTimeOffset CreatedAt);

    [Test]
    public async Task ListCompanies_returns_all_for_superadmin()
    {
        await factory.SeedCompanyAsync(legalName: "Alpha Co", taxId: "1");
        await factory.SeedCompanyAsync(legalName: "Beta Co", taxId: "2");
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.GetAsync("/api/companies");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<CompanyItem>>();
        body.Should().NotBeNull().And.HaveCount(2);
        body!.Select(x => x.LegalName).Should().BeEquivalentTo(new[] { "Alpha Co", "Beta Co" });
    }

    [Test]
    public async Task ListCompanies_non_superadmin_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: Guid.NewGuid(), permissions: [Permissions.UsersManage]);

        var response = await client.GetAsync("/api/companies");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ListCompanies_anonymous_returns_401()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/companies");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
