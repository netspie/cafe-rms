using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Tags;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class GetTagByIdTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record TagDetail(Guid Id, string Name, string? ImageUrl, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    [Test]
    public async Task GetTagById_happy_path_returns_tag()
    {
        var company = await factory.SeedCompanyAsync();
        var tagId = await SeedTagAsync(company.Id, "Vegan", "https://cdn/v.png");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync($"/api/tags/{tagId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TagDetail>();
        body!.Name.Should().Be("Vegan");
        body.ImageUrl.Should().Be("https://cdn/v.png");
    }

    [Test]
    public async Task GetTagById_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync($"/api/tags/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedTagAsync(Guid companyId, string name, string? imageUrl = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tag = Tag.Create(name, companyId, imageUrl);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return tag.Id;
    }
}
