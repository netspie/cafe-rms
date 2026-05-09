using System.Net;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tables;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Tables;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteTableTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeleteTable_happy_path_returns_204()
    {
        var id = await SeedTableAsync("Table 1");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TablesManage]);

        var response = await client.DeleteAsync($"/api/tables/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteTable_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TablesManage]);

        var response = await client.DeleteAsync($"/api/tables/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedTableAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outletId = await db.Outlets.Select(x => x.Id).FirstOrDefaultAsync();
        if (outletId == Guid.Empty)
        {
            var seeded = await factory.SeedOutletAsync();
            outletId = seeded.Id;
        }
        var table = Table.Create(name, outletId);
        db.Tables.Add(table);
        await db.SaveChangesAsync();
        return table.Id;
    }
}
