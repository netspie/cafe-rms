using System.Net;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.TaxRates;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteTaxRateTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeleteTaxRate_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var id = await SeedTaxRateAsync(company.Id, "VAT 23%", "x", 23m);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TaxRatesManage]);

        var response = await client.DeleteAsync($"/api/tax-rates/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteTaxRate_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TaxRatesManage]);

        var response = await client.DeleteAsync($"/api/tax-rates/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedTaxRateAsync(Guid companyId, string name, string description, decimal rate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = TaxRate.Create(name, description, rate, companyId);
        db.TaxRates.Add(taxRate);
        await db.SaveChangesAsync();
        return taxRate.Id;
    }
}
