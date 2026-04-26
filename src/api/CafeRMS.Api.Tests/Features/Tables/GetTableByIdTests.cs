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
public sealed class GetTableByIdTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record TableDetail(Guid Id, string Name, Guid OutletId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    [Test]
    public async Task GetTableById_happy_path_returns_table()
    {
        var company = await factory.SeedCompanyAsync();
        var id = await SeedTableAsync(company.Id, "Table 1");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TablesManage]);

        var response = await client.GetAsync($"/api/tables/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TableDetail>();
        body!.Name.Should().Be("Table 1");
    }

    [Test]
    public async Task GetTableById_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TablesManage]);

        var response = await client.GetAsync($"/api/tables/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedTableAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outletId = await db.Outlets.IgnoreQueryFilters()
            .Where(x => x.CompanyId == companyId)
            .Select(x => x.Id)
            .FirstAsync();
        var table = Table.Create(name, outletId, companyId);
        db.Tables.Add(table);
        await db.SaveChangesAsync();
        return table.Id;
    }
}
