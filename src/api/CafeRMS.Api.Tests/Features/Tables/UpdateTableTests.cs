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
public sealed class UpdateTableTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UpdateTable_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var id = await SeedTableAsync(company.Id, "Table 1");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TablesManage]);

        var response = await client.PutAsJsonAsync($"/api/tables/{id}", new { name = "Window seat" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateTable_duplicate_name_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        await SeedTableAsync(company.Id, "Bar");
        var t1Id = await SeedTableAsync(company.Id, "Table 1");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.TablesManage]);

        var response = await client.PutAsJsonAsync($"/api/tables/{t1Id}", new { name = "Bar" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
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
