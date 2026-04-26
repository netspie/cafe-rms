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
        var company = await factory.SeedCompanyAsync();
        await SeedPriceGroupAsync(company.Id, "Standard");
        await SeedPriceGroupAsync(company.Id, "Member");
        await SeedPriceGroupAsync(company.Id, "Event");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PricingManage]);

        var response = await client.GetAsync("/api/price-groups?sort=name");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("Event", "Member", "Standard");
    }

    [Test]
    public async Task ListPriceGroups_does_not_include_other_companies_items()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await SeedPriceGroupAsync(ownCompany.Id, "OurStd");
        await SeedPriceGroupAsync(otherCompany.Id, "TheirStd");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.PricingManage]);

        var response = await client.GetAsync("/api/price-groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurStd");
    }

    private async Task SeedPriceGroupAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PriceGroups.Add(PriceGroup.Create(name, companyId));
        await db.SaveChangesAsync();
    }
}
