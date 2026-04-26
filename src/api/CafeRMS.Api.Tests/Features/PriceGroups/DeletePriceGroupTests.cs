using System.Net;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.SalesChannels;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.PriceGroups;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeletePriceGroupTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeletePriceGroup_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var id = await SeedPriceGroupAsync(company.Id, "Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PricingManage]);

        var response = await client.DeleteAsync($"/api/price-groups/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeletePriceGroup_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PricingManage]);

        var response = await client.DeleteAsync($"/api/price-groups/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeletePriceGroup_hard_deletes_sales_channel_links()
    {
        var company = await factory.SeedCompanyAsync();
        var pgId = await SeedPriceGroupAsync(company.Id, "Standard");
        var scId = await SeedSalesChannelAsync(company.Id, "Dine-in");
        await SeedLinkAsync(scId, pgId);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PricingManage]);

        var response = await client.DeleteAsync($"/api/price-groups/{pgId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingLinks = await db.SalesChannelPriceGroups.IgnoreQueryFilters().CountAsync(x => x.PriceGroupId == pgId);
        remainingLinks.Should().Be(0);
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

    private async Task<Guid> SeedSalesChannelAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sc = SalesChannel.Create(name, false, companyId);
        db.SalesChannels.Add(sc);
        await db.SaveChangesAsync();
        return sc.Id;
    }

    private async Task SeedLinkAsync(Guid salesChannelId, Guid priceGroupId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.SalesChannelPriceGroups.Add(SalesChannelPriceGroup.Create(salesChannelId, priceGroupId));
        await db.SaveChangesAsync();
    }
}
