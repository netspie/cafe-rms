using System.Net;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Tags;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteTagTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeleteTag_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var tagId = await SeedTagAsync(company.Id, "Vegan");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/tags/{tagId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteTag_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ProductsManage]);

        var response = await client.DeleteAsync($"/api/tags/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
