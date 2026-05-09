using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Allergens;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Allergens;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListAllergensTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record AllergenItem(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<AllergenItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task ListAllergens_filter_and_sort_returns_matching_items()
    {
        await SeedAllergenAsync("Peanuts");
        await SeedAllergenAsync("Pecans");
        await SeedAllergenAsync("Gluten");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.GetAsync("/api/allergens?name=pe&sort=-name");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(2);
        body.Items.Select(x => x.Name).Should().Equal("Pecans", "Peanuts");
    }

    private async Task SeedAllergenAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Allergens.Add(Allergen.Create(name));
        await db.SaveChangesAsync();
    }
}
