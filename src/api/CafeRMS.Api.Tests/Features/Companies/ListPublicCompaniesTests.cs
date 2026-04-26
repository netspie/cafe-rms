using System.Net;
using System.Net.Http.Json;

namespace CafeRMS.Api.Tests.Features.Companies;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ListPublicCompaniesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record PublicItem(Guid Id, string LegalName, string DisplayName, string StreetAddress, string Phone, string TimeZone, string Currency, string? LogoUrl);

    [Test]
    public async Task ListPublic_returns_only_isPublic_true_companies()
    {
        await factory.SeedCompanyAsync(legalName: "Public Co", taxId: "1", isPublic: true);
        await factory.SeedCompanyAsync(legalName: "Private Co", taxId: "2", isPublic: false);
        using var client = factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/companies/public");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<PublicItem>>();
        body.Should().NotBeNull().And.HaveCount(1);
        body![0].LegalName.Should().Be("Public Co");
    }

    [Test]
    public async Task ListPublic_works_anonymously()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/companies/public");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
