using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tables;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Tables;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListTablesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record TableItem(Guid Id, string Name, Guid OutletId, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<TableItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task ListTables_filter_and_sort_returns_matching_items()
    {
        var company = await factory.SeedCompanyAsync();
        await SeedTableAsync(company.Id, "Patio 1");
        await SeedTableAsync(company.Id, "Patio 2");
        await SeedTableAsync(company.Id, "Bar");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TablesManage]);

        var response = await client.GetAsync("/api/tables?name=patio&sort=-name");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(2);
        body.Items.Select(x => x.Name).Should().Equal("Patio 2", "Patio 1");
    }

    [Test]
    public async Task ListTables_does_not_include_other_companies_items()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await SeedTableAsync(ownCompany.Id, "OurTable");
        await SeedTableAsync(otherCompany.Id, "TheirTable");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.TablesManage]);

        var response = await client.GetAsync("/api/tables");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurTable");
    }

    private async Task SeedTableAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outletId = await db.Outlets.IgnoreQueryFilters()
            .Where(x => x.CompanyId == companyId)
            .Select(x => x.Id)
            .FirstAsync();
        db.Tables.Add(Table.Create(name, outletId, companyId));
        await db.SaveChangesAsync();
    }
}
