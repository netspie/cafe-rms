using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Companies;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UpdateCompanyTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private static object Payload(string taxId = "9999999999", bool isPublic = true) => new
    {
        legalName = "Updated Co",
        taxId,
        invoicingAddress = "ul. New 1, 00-001 Warsaw",
        billingEmail = "new@billing.local",
        billingPhone = "+48999888777",
        isPublic
    };

    [Test]
    public async Task Update_happy_path_returns_204_and_flips_isPublic()
    {
        var company = await factory.SeedCompanyAsync(taxId: "1234567890", isPublic: false);
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.PutAsJsonAsync($"/api/companies/{company.Id}", Payload(isPublic: true));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Update_non_superadmin_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id);

        var response = await client.PutAsJsonAsync($"/api/companies/{company.Id}", Payload());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Update_taxId_collision_with_other_company_returns_409()
    {
        var c1 = await factory.SeedCompanyAsync(legalName: "Co 1", taxId: "111");
        var c2 = await factory.SeedCompanyAsync(legalName: "Co 2", taxId: "222");
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.PutAsJsonAsync($"/api/companies/{c2.Id}", Payload(taxId: "111"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Update_returns_404_for_nonexistent_id()
    {
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.PutAsJsonAsync($"/api/companies/{Guid.NewGuid()}", Payload());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
