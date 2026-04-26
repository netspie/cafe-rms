using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Tags;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UpdateTagTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task UpdateTag_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var tagId = await SeedTagAsync(company.Id, "Vegan");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PutAsJsonAsync($"/api/tags/{tagId}", new { name = "Plant-based", imageUrl = "https://cdn/p.png" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task UpdateTag_duplicate_name_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        await SeedTagAsync(company.Id, "Spicy");
        var veganId = await SeedTagAsync(company.Id, "Vegan");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.PutAsJsonAsync($"/api/tags/{veganId}", new { name = "Spicy", imageUrl = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> SeedTagAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tag = Tag.Create(name, companyId);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return tag.Id;
    }
}
