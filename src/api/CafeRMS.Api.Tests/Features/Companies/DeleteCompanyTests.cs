using System.Net;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Companies;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteCompanyTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task Delete_happy_path_returns_204_and_soft_deletes()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.DeleteAsync($"/api/companies/{company.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Subsequent GET as SuperAdmin should now 404 — the soft-delete query filter hides it.
        var followup = await client.GetAsync($"/api/companies/{company.Id}");
        followup.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Delete_non_superadmin_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id);

        var response = await client.DeleteAsync($"/api/companies/{company.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Delete_returns_404_for_nonexistent_id()
    {
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.DeleteAsync($"/api/companies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
