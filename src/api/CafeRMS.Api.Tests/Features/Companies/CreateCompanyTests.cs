using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Companies;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class CreateCompanyTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private static object SamplePayload(string taxId = "1234567890", string ownerEmail = "owner@new.local") => new
    {
        legalName = "Acme Cafe Sp. z o.o.",
        taxId,
        invoicingAddress = "ul. Acme 1, 00-001 Warsaw",
        billingEmail = "billing@acme.local",
        billingPhone = "+48111222333",
        isPublic = false,
        outletDisplayName = "Acme Cafe",
        outletStreetAddress = "ul. Acme 1, 00-001 Warsaw",
        outletPhone = "+48111222333",
        outletTimeZone = "Europe/Warsaw",
        outletCurrency = "PLN",
        outletLogoUrl = (string?)null,
        ownerEmail,
        ownerPassword = "OwnerPass1!",
        ownerFirstName = "First",
        ownerLastName = "Owner"
    };

    private sealed record CreateCompanyResponseBody(Guid CompanyId, Guid OutletId, Guid OwnerRoleId, Guid OwnerUserId);

    private sealed record LoginResponseBody(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

    [Test]
    public async Task CreateCompany_happy_path_returns_four_ids()
    {
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.PostAsJsonAsync("/api/companies", SamplePayload());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CreateCompanyResponseBody>();
        body.Should().NotBeNull();
        body!.CompanyId.Should().NotBe(Guid.Empty);
        body.OutletId.Should().NotBe(Guid.Empty);
        body.OwnerRoleId.Should().NotBe(Guid.Empty);
        body.OwnerUserId.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task CreateCompany_then_owner_can_login()
    {
        using var super = factory.CreateClientAs(AccountType.SuperAdmin);

        var create = await super.PostAsJsonAsync("/api/companies", SamplePayload(ownerEmail: "owner@new.local"));
        create.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = (await create.Content.ReadFromJsonAsync<CreateCompanyResponseBody>())!;

        using var anon = factory.CreateAnonymousClient();
        var login = await anon.PostAsJsonAsync("/api/auth/login", new { email = "owner@new.local", password = "OwnerPass1!" });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await login.Content.ReadFromJsonAsync<LoginResponseBody>();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.AccountType.Should().Be("Staff");
        // The new owner-staff is tied to the just-created company; companyId is in the JWT claim,
        // verified end-to-end by hitting an authenticated endpoint:
        using var ownerClient = factory.CreateAnonymousClient();
        ownerClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
        var meRoles = await ownerClient.GetAsync("/api/roles");
        // Owner role bypass means the Owner staff has RolesManage access via the policy handler.
        meRoles.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task CreateCompany_non_superadmin_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: Guid.NewGuid(), permissions: [Permissions.UsersManage]);

        var response = await client.PostAsJsonAsync("/api/companies", SamplePayload());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task CreateCompany_anonymous_returns_401()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/companies", SamplePayload());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task CreateCompany_duplicate_taxId_returns_409()
    {
        await factory.SeedCompanyAsync(taxId: "1234567890");
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.PostAsJsonAsync("/api/companies", SamplePayload(taxId: "1234567890"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task CreateCompany_duplicate_owner_email_returns_409()
    {
        await factory.SeedUserAsync(AccountType.Guest, "taken@x.local", "Pass1234!");
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.PostAsJsonAsync("/api/companies", SamplePayload(ownerEmail: "taken@x.local"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
