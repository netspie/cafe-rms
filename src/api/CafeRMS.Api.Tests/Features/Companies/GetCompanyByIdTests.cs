using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Companies;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class GetCompanyByIdTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record OutletInfo(Guid Id, string DisplayName, string StreetAddress, string Phone, string TimeZone, string Currency, string? LogoUrl);
    private sealed record CompanyDetail(Guid Id, string LegalName, string TaxId, string InvoicingAddress, string BillingEmail, string BillingPhone, bool IsPublic, DateTimeOffset CreatedAt, OutletInfo Outlet);

    [Test]
    public async Task GetById_superadmin_can_get_any()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.GetAsync($"/api/companies/{company.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CompanyDetail>();
        body!.Id.Should().Be(company.Id);
        body.Outlet.DisplayName.Should().Be("Test Outlet");
    }

    [Test]
    public async Task GetById_staff_can_get_own_company()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id);

        var response = await client.GetAsync($"/api/companies/{company.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetById_staff_cannot_get_other_company_returns_403()
    {
        var ownCompany = await factory.SeedCompanyAsync(taxId: "111");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other Co", taxId: "222");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id);

        var response = await client.GetAsync($"/api/companies/{otherCompany.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_404_for_nonexistent_id()
    {
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.GetAsync($"/api/companies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
