using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Allergens;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Allergens;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UpdateAllergenTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UpdateAllergen_happy_path_returns_204()
    {
        var id = await SeedAllergenAsync("Peanuts");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PutAsJsonAsync($"/api/allergens/{id}", new { name = "Tree nuts" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateAllergen_duplicate_name_returns_409()
    {
        await SeedAllergenAsync("Gluten");
        var peanutsId = await SeedAllergenAsync("Peanuts");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.PutAsJsonAsync($"/api/allergens/{peanutsId}", new { name = "Gluten" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> SeedAllergenAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var allergen = Allergen.Create(name);
        db.Allergens.Add(allergen);
        await db.SaveChangesAsync();
        return allergen.Id;
    }
}
