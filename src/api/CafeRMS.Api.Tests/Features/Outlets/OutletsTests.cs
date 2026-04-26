using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Outlets;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Outlets;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class OutletsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record OutletDetail(
        Guid Id, string DisplayName, string StreetAddress, string Phone, string TimeZone,
        string Currency, string? LogoUrl, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    [Test]
    public async Task GetById_happy_path_returns_outlet()
    {
        var company = await factory.SeedCompanyAsync();
        var outletId = await GetCompanyOutletIdAsync(company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.OutletManage]);

        var response = await client.GetAsync($"/api/outlets/{outletId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OutletDetail>();
        body!.DisplayName.Should().Be("Test Outlet");
        body.Currency.Should().Be("PLN");
    }

    [Test]
    public async Task GetById_other_companys_outlet_returns_404()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        var otherOutletId = await GetCompanyOutletIdAsync(otherCompany.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.OutletManage]);

        var response = await client.GetAsync($"/api/outlets/{otherOutletId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var outletId = await GetCompanyOutletIdAsync(company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.OutletManage]);

        var response = await client.PutAsJsonAsync($"/api/outlets/{outletId}", new
        {
            displayName = "Cafe Bean",
            streetAddress = "ul. Nowa 5, 00-002 Warsaw",
            phone = "+48555000000",
            timeZone = "Europe/Warsaw",
            currency = "EUR",
            logoUrl = "https://cdn/logo.png"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Update_without_OutletManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var outletId = await GetCompanyOutletIdAsync(company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PutAsJsonAsync($"/api/outlets/{outletId}", new
        {
            displayName = "X", streetAddress = "x", phone = "x", timeZone = "x",
            currency = "PLN", logoUrl = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Guid> GetCompanyOutletIdAsync(Guid companyId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Outlets.IgnoreQueryFilters()
            .Where(x => x.CompanyId == companyId)
            .Select(x => x.Id)
            .FirstAsync();
    }
}
