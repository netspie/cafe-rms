using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.PriceGroups;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UpdatePriceGroupTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UpdatePriceGroup_happy_path_returns_204()
    {
        var id = await SeedPriceGroupAsync("Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PricingManage]);

        var response = await client.PutAsJsonAsync($"/api/price-groups/{id}", new { name = "Member" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdatePriceGroup_duplicate_name_returns_409()
    {
        await SeedPriceGroupAsync("Member");
        var stdId = await SeedPriceGroupAsync("Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PricingManage]);

        var response = await client.PutAsJsonAsync($"/api/price-groups/{stdId}", new { name = "Member" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> SeedPriceGroupAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pg = PriceGroup.Create(name);
        db.PriceGroups.Add(pg);
        await db.SaveChangesAsync();
        return pg.Id;
    }
}
