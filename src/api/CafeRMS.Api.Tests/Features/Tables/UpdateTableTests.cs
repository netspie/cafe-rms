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
        var id = await SeedTableAsync("Table 1");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TablesManage]);

        var response = await client.PutAsJsonAsync($"/api/tables/{id}", new { name = "Window seat" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateTable_duplicate_name_returns_409()
    {
        await SeedTableAsync("Bar");
        var t1Id = await SeedTableAsync("Table 1");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TablesManage]);

        var response = await client.PutAsJsonAsync($"/api/tables/{t1Id}", new { name = "Bar" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
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
