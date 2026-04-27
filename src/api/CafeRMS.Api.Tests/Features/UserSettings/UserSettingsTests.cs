using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.UserSettings;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UserSettingsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record SettingsResp(string Theme, string UiSettingsJson);

    [Test]
    public async Task Get_auto_creates_with_defaults()
    {
        var userId = Guid.NewGuid();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var response = await client.GetAsync("/api/my/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SettingsResp>();
        body!.Theme.Should().Be("light");
        body.UiSettingsJson.Should().Be("{}");
    }

    [Test]
    public async Task Put_then_Get_round_trip()
    {
        var userId = Guid.NewGuid();
        using var client = factory.CreateClientAs(AccountType.Guest, userId: userId);

        var put = await client.PutAsJsonAsync("/api/my/settings", new { theme = "dark", uiSettingsJson = "{\"sidebar\":\"collapsed\"}" });
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await client.GetAsync("/api/my/settings");
        var body = await get.Content.ReadFromJsonAsync<SettingsResp>();
        body!.Theme.Should().Be("dark");
        body.UiSettingsJson.Should().Be("{\"sidebar\":\"collapsed\"}");
    }
}
