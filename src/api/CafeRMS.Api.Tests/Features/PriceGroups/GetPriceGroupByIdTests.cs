using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.PriceGroups;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class GetPriceGroupByIdTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record PriceGroupDetail(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    [Test]
    public async Task GetPriceGroupById_happy_path_returns_price_group()
    {
        var company = await factory.SeedCompanyAsync();
        var id = await SeedPriceGroupAsync(company.Id, "Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PricingManage]);

        var response = await client.GetAsync($"/api/price-groups/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PriceGroupDetail>();
        body!.Name.Should().Be("Standard");
    }

    [Test]
    public async Task GetPriceGroupById_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PricingManage]);

        var response = await client.GetAsync($"/api/price-groups/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedPriceGroupAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pg = PriceGroup.Create(name, companyId);
        db.PriceGroups.Add(pg);
        await db.SaveChangesAsync();
        return pg.Id;
    }
}
