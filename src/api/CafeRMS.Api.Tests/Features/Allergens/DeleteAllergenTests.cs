using System.Net;
using CafeRMS.Api.Features.Allergens;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Allergens;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteAllergenTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeleteAllergen_happy_path_returns_204()
    {
        var id = await SeedAllergenAsync("Peanuts");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/allergens/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteAllergen_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/allergens/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
