using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.TaxRates;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class GetTaxRateByIdTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record TaxRateDetail(Guid Id, string Name, string Description, decimal Rate, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    [Test]
    public async Task GetTaxRateById_happy_path_returns_tax_rate()
    {
        var id = await SeedTaxRateAsync("VAT 23%", "Standard", 23m);
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TaxRatesManage]);

        var response = await client.GetAsync($"/api/tax-rates/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TaxRateDetail>();
        body!.Name.Should().Be("VAT 23%");
        body.Rate.Should().Be(23m);
    }

    [Test]
    public async Task GetTaxRateById_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TaxRatesManage]);

        var response = await client.GetAsync($"/api/tax-rates/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
