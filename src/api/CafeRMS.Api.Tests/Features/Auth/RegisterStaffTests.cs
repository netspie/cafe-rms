using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class RegisterStaffTests : IDisposable
{
    private static readonly Guid CompanyA = Guid.Parse("00000000-0000-0000-0000-00000000000a");

    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task RegisterStaff_happy_path_creates_user_with_assigned_roles()
    {
        await factory.SeedRoleAsync("Cashier", CompanyA);

        using var client = factory.CreateClientAs(
            AccountType.Staff,
            companyId: CompanyA,
            permissions: [Permissions.UsersManage]);

        var response = await client.PostAsJsonAsync("/api/auth/register/staff", new
        {
            email = "newstaff@test.local",
            password = "Pass1234!",
            firstName = "New",
            lastName = "Staff",
            roles = new[] { "Cashier" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task RegisterStaff_without_UsersManage_returns_403()
    {
        using var client = factory.CreateClientAs(
            AccountType.Staff,
            companyId: CompanyA,
            permissions: []);

        var response = await client.PostAsJsonAsync("/api/auth/register/staff", new
        {
            email = "blocked@test.local",
            password = "Pass1234!",
            firstName = "B",
            lastName = "L",
            roles = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task RegisterStaff_assigning_Owner_role_returns_403()
    {
        using var client = factory.CreateClientAs(
            AccountType.Staff,
            companyId: CompanyA,
            permissions: [Permissions.UsersManage]);

        var response = await client.PostAsJsonAsync("/api/auth/register/staff", new
        {
            email = "tryowner@test.local",
            password = "Pass1234!",
            firstName = "Try",
            lastName = "Owner",
            roles = new[] { "Owner" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task RegisterStaff_without_company_context_returns_403()
    {
        // SuperAdmin without X-Company-Id header → CurrentCompanyId resolves to Guid.Empty
        // → use case throws ForbiddenException("A company context is required").
        using var client = factory.CreateClientAs(AccountType.SuperAdmin);

        var response = await client.PostAsJsonAsync("/api/auth/register/staff", new
        {
            email = "nocontext@test.local",
            password = "Pass1234!",
            firstName = "No",
            lastName = "Context",
            roles = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
