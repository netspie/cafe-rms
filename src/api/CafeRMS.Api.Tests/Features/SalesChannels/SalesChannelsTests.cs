using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups;
using CafeRMS.Api.Features.SalesChannels;
using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.SalesChannels;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class SalesChannelsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record SalesChannelDetail(
        Guid Id, string Name, bool IsTakeout, IReadOnlyList<Guid> PriceGroupIds,
        DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record SalesChannelItem(Guid Id, string Name, bool IsTakeout, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<SalesChannelItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.PostAsJsonAsync("/api/sales-channels", new { name = "Dine-in", isTakeout = false });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_duplicate_name_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        await SeedChannelAsync(company.Id, "Dine-in", false);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.PostAsJsonAsync("/api/sales-channels", new { name = "Dine-in", isTakeout = true });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_without_permission_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PostAsJsonAsync("/api/sales-channels", new { name = "Dine-in", isTakeout = false });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_channel_with_linked_price_groups()
    {
        var company = await factory.SeedCompanyAsync();
        var scId = await SeedChannelAsync(company.Id, "Dine-in", false);
        var pgId = await SeedPriceGroupAsync(company.Id, "Standard");
        await SeedLinkAsync(scId, pgId);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.GetAsync($"/api/sales-channels/{scId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalesChannelDetail>();
        body!.Name.Should().Be("Dine-in");
        body.PriceGroupIds.Should().Equal(pgId);
    }

    [Test]
    public async Task GetById_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.GetAsync($"/api/sales-channels/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task List_does_not_include_other_companies_items()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await SeedChannelAsync(ownCompany.Id, "OurChannel", false);
        await SeedChannelAsync(otherCompany.Id, "TheirChannel", false);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.GetAsync("/api/sales-channels");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurChannel");
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var scId = await SeedChannelAsync(company.Id, "Dine-in", false);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.PutAsJsonAsync($"/api/sales-channels/{scId}", new { name = "Takeout", isTakeout = true });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_hard_deletes_links()
    {
        var company = await factory.SeedCompanyAsync();
        var scId = await SeedChannelAsync(company.Id, "Dine-in", false);
        var pgId = await SeedPriceGroupAsync(company.Id, "Standard");
        await SeedLinkAsync(scId, pgId);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.DeleteAsync($"/api/sales-channels/{scId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var leftover = await db.SalesChannelPriceGroups.IgnoreQueryFilters().CountAsync(x => x.SalesChannelId == scId);
        leftover.Should().Be(0);
    }

    [Test]
    public async Task Link_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var scId = await SeedChannelAsync(company.Id, "Dine-in", false);
        var pgId = await SeedPriceGroupAsync(company.Id, "Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.PostAsync($"/api/sales-channels/{scId}/price-groups/{pgId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Link_is_idempotent()
    {
        var company = await factory.SeedCompanyAsync();
        var scId = await SeedChannelAsync(company.Id, "Dine-in", false);
        var pgId = await SeedPriceGroupAsync(company.Id, "Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);
        await client.PostAsync($"/api/sales-channels/{scId}/price-groups/{pgId}", null);

        var response = await client.PostAsync($"/api/sales-channels/{scId}/price-groups/{pgId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Link_with_unknown_price_group_returns_404()
    {
        var company = await factory.SeedCompanyAsync();
        var scId = await SeedChannelAsync(company.Id, "Dine-in", false);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.PostAsync($"/api/sales-channels/{scId}/price-groups/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Unlink_returns_404_when_link_missing()
    {
        var company = await factory.SeedCompanyAsync();
        var scId = await SeedChannelAsync(company.Id, "Dine-in", false);
        var pgId = await SeedPriceGroupAsync(company.Id, "Standard");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.SalesChannelsManage]);

        var response = await client.DeleteAsync($"/api/sales-channels/{scId}/price-groups/{pgId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedChannelAsync(Guid companyId, string name, bool isTakeout)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sc = SalesChannel.Create(name, isTakeout, companyId);
        db.SalesChannels.Add(sc);
        await db.SaveChangesAsync();
        return sc.Id;
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

    private async Task SeedLinkAsync(Guid salesChannelId, Guid priceGroupId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.SalesChannelPriceGroups.Add(SalesChannelPriceGroup.Create(salesChannelId, priceGroupId));
        await db.SaveChangesAsync();
    }
}
