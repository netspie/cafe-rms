using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ChangePasswordTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task ChangePassword_happy_path_returns_204_and_lets_user_login_with_new_password()
    {
        var user = await factory.SeedUserAsync(AccountType.Guest, "pwchange@test.local", "OldPass1!");
        using var authed = factory.CreateClientAs(AccountType.Guest, userId: user.Id);

        var response = await authed.PutAsJsonAsync("/api/auth/password", new
        {
            currentPassword = "OldPass1!",
            newPassword = "NewPass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var anon = factory.CreateAnonymousClient();

        // Old password no longer works.
        var oldLogin = await anon.PostAsJsonAsync("/api/auth/login", new { email = "pwchange@test.local", password = "OldPass1!" });
        oldLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // New password does.
        var newLogin = await anon.PostAsJsonAsync("/api/auth/login", new { email = "pwchange@test.local", password = "NewPass1!" });
        newLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task ChangePassword_with_wrong_current_returns_401()
    {
        var user = await factory.SeedUserAsync(AccountType.Guest, "wrong@test.local", "OldPass1!");
        using var authed = factory.CreateClientAs(AccountType.Guest, userId: user.Id);

        var response = await authed.PutAsJsonAsync("/api/auth/password", new
        {
            currentPassword = "NotTheRightOne1!",
            newPassword = "NewPass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ChangePassword_with_weak_new_returns_400()
    {
        var user = await factory.SeedUserAsync(AccountType.Guest, "weaknew@test.local", "OldPass1!");
        using var authed = factory.CreateClientAs(AccountType.Guest, userId: user.Id);

        var response = await authed.PutAsJsonAsync("/api/auth/password", new
        {
            currentPassword = "OldPass1!",
            newPassword = "weakpass"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ChangePassword_anonymous_returns_401()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.PutAsJsonAsync("/api/auth/password", new
        {
            currentPassword = "x",
            newPassword = "Pass1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
