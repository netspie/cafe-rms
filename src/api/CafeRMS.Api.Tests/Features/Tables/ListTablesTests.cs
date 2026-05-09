using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tables;
using CafeRMS.Api.Persistence;
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
        var outlet = await factory.SeedOutletAsync();
        await SeedTableAsync(outlet.Id, "Patio 1");
        await SeedTableAsync(outlet.Id, "Patio 2");
        await SeedTableAsync(outlet.Id, "Bar");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.TablesManage]);

        var response = await client.GetAsync("/api/tables?name=patio&sort=-name");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(2);
        body.Items.Select(x => x.Name).Should().Equal("Patio 2", "Patio 1");
    }

    private async Task SeedTableAsync(Guid outletId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Tables.Add(Table.Create(name, outletId));
        await db.SaveChangesAsync();
    }
}
