using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.TaxRates;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class AddTaxRateTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task AddTaxRate_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TaxRatesManage]);

        var response = await client.PostAsJsonAsync("/api/tax-rates", new { name = "VAT 23%", description = "Standard VAT", rate = 23m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task AddTaxRate_duplicate_name_returns_409()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TaxRatesManage]);
        await client.PostAsJsonAsync("/api/tax-rates", new { name = "VAT 23%", description = "Std", rate = 23m });

        var response = await client.PostAsJsonAsync("/api/tax-rates", new { name = "VAT 23%", description = "Std", rate = 23m });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddTaxRate_without_TaxRatesManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/tax-rates", new { name = "VAT", description = "x", rate = 23m });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
