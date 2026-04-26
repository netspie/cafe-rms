using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class LoginTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record LoginResponseBody(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

    [Test]
    public async Task Login_with_correct_credentials_returns_jwt()
    {
        await factory.SeedUserAsync(AccountType.Guest, "guest@test.local", "Pass1234!");
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "guest@test.local", password = "Pass1234!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.AccountType.Should().Be("Guest");
        body.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Test]
    public async Task Login_with_wrong_password_returns_401()
    {
        await factory.SeedUserAsync(AccountType.Guest, "guest@test.local", "Pass1234!");
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "guest@test.local", password = "WrongPass1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_with_unknown_email_returns_401()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "ghost@nowhere.local", password = "Pass1234!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_with_invalid_email_format_returns_400()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "not-an-email", password = "Pass1234!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
