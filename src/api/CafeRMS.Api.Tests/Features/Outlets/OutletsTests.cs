using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Outlets;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class OutletsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record OutletDetail(
        Guid Id, string DisplayName, string StreetAddress, string Phone, string TimeZone,
        string Currency, string? LogoUrl, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    [Test]
    public async Task GetById_happy_path_returns_outlet()
    {
        var outlet = await factory.SeedOutletAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.OutletManage]);

        var response = await client.GetAsync($"/api/outlets/{outlet.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OutletDetail>();
        body!.DisplayName.Should().Be("Test Outlet");
        body.Currency.Should().Be("PLN");
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var outlet = await factory.SeedOutletAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.OutletManage]);

        var response = await client.PutAsJsonAsync($"/api/outlets/{outlet.Id}", new
        {
            displayName = "Cafe Bean",
            streetAddress = "ul. Nowa 5, 00-002 Warsaw",
            phone = "+48555000000",
            timeZone = "Europe/Warsaw",
            currency = "EUR",
            logoUrl = "https://cdn/logo.png"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Update_without_OutletManage_returns_403()
    {
        var outlet = await factory.SeedOutletAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PutAsJsonAsync($"/api/outlets/{outlet.Id}", new
        {
            displayName = "X", streetAddress = "x", phone = "x", timeZone = "x",
            currency = "PLN", logoUrl = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
