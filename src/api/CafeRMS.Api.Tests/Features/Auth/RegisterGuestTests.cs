using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class RegisterGuestTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record GuestResponseBody(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

    [Test]
    public async Task RegisterGuest_happy_path_returns_jwt_and_creates_user()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/register/guest", new
        {
            email = "newguest@test.local",
            password = "Pass1234!",
            firstName = "New",
            lastName = "Guest"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GuestResponseBody>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.AccountType.Should().Be("Guest");
    }

    [Test]
    public async Task RegisterGuest_duplicate_email_returns_409()
    {
        await factory.SeedUserAsync(AccountType.Guest, "taken@test.local", "Pass1234!");
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/register/guest", new
        {
            email = "taken@test.local",
            password = "Pass1234!",
            firstName = "Dup",
            lastName = "Email"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task RegisterGuest_weak_password_returns_400_from_identity_rules()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/register/guest", new
        {
            email = "weak@test.local",
            password = "abc12345",   // 8 chars but no uppercase — fails Identity complexity
            firstName = "Weak",
            lastName = "Pass"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
