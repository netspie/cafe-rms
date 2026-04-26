using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Tags;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListTagsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record TagItem(Guid Id, string Name, string? ImageUrl, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<TagItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task ListTags_filter_and_sort_returns_matching_items()
    {
        var company = await factory.SeedCompanyAsync();
        await SeedTagAsync(company.Id, "Vegan");
        await SeedTagAsync(company.Id, "Vegetarian");
        await SeedTagAsync(company.Id, "Spicy");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync("/api/tags?name=veg&sort=-name");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(2);
        body.Items.Select(x => x.Name).Should().Equal("Vegetarian", "Vegan");
    }

    [Test]
    public async Task ListTags_does_not_include_other_companies_tags()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await SeedTagAsync(ownCompany.Id, "OurTag");
        await SeedTagAsync(otherCompany.Id, "TheirTag");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync("/api/tags");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurTag");
    }

    private async Task SeedTagAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Tags.Add(Tag.Create(name, companyId));
        await db.SaveChangesAsync();
    }
}
