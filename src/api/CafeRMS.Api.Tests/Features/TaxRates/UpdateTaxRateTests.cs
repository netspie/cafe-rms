using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.TaxRates;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UpdateTaxRateTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UpdateTaxRate_happy_path_returns_204()
    {
        var id = await SeedTaxRateAsync("VAT 23%", "Standard", 23m);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TaxRatesManage]);

        var response = await client.PutAsJsonAsync($"/api/tax-rates/{id}", new { name = "VAT 8%", description = "Reduced", rate = 8m });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateTaxRate_duplicate_name_returns_409()
    {
        await SeedTaxRateAsync("Reduced", "x", 8m);
        var stdId = await SeedTaxRateAsync("Standard", "y", 23m);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TaxRatesManage]);

        var response = await client.PutAsJsonAsync($"/api/tax-rates/{stdId}", new { name = "Reduced", description = "y", rate = 23m });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> SeedTaxRateAsync(string name, string description, decimal rate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var taxRate = TaxRate.Create(name, description, rate);
        db.TaxRates.Add(taxRate);
        await db.SaveChangesAsync();
        return taxRate.Id;
    }
}
