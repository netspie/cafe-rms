using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.TaxRates;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.TaxRates;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListTaxRatesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record TaxRateItem(Guid Id, string Name, string Description, decimal Rate, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<TaxRateItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task ListTaxRates_sort_by_rate_descending()
    {
        var company = await factory.SeedCompanyAsync();
        await SeedTaxRateAsync(company.Id, "Zero", "0%", 0m);
        await SeedTaxRateAsync(company.Id, "Reduced", "8%", 8m);
        await SeedTaxRateAsync(company.Id, "Standard", "23%", 23m);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TaxRatesManage]);

        var response = await client.GetAsync("/api/tax-rates?sort=-rate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Rate).Should().Equal(23m, 8m, 0m);
    }

    [Test]
    public async Task ListTaxRates_does_not_include_other_companies_items()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await SeedTaxRateAsync(ownCompany.Id, "OurVat", "x", 23m);
        await SeedTaxRateAsync(otherCompany.Id, "TheirVat", "y", 19m);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.TaxRatesManage]);

        var response = await client.GetAsync("/api/tax-rates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurVat");
    }

    private async Task SeedTaxRateAsync(Guid companyId, string name, string description, decimal rate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.TaxRates.Add(TaxRate.Create(name, description, rate, companyId));
        await db.SaveChangesAsync();
    }
}
