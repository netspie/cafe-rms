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
        await SeedTagAsync("Vegan");
        await SeedTagAsync("Vegetarian");
        await SeedTagAsync("Spicy");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync("/api/tags?name=veg&sort=-name");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(2);
        body.Items.Select(x => x.Name).Should().Equal("Vegetarian", "Vegan");
    }

    private async Task SeedTagAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Tags.Add(Tag.Create(name));
        await db.SaveChangesAsync();
    }
}
