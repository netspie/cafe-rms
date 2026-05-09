using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.PriceGroups;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListPriceGroupsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record PriceGroupItem(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<PriceGroupItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task ListPriceGroups_filter_and_sort_returns_matching_items()
    {
        await SeedPriceGroupAsync("Standard");
        await SeedPriceGroupAsync("Member");
        await SeedPriceGroupAsync("Event");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PricingManage]);

        var response = await client.GetAsync("/api/price-groups?sort=name");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("Event", "Member", "Standard");
    }

    private async Task SeedPriceGroupAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PriceGroups.Add(PriceGroup.Create(name));
        await db.SaveChangesAsync();
    }
}
