using System.Net;
using CafeRMS.Api.Features.Auth;

namespace CafeRMS.Api.Tests.Features.Auth;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class DeleteUserTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Test]
    public async Task DeleteUser_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var target = await factory.SeedUserAsync(AccountType.Staff, "target@x.local", "Pass1234!", companyId: company.Id);
        // Acting user is a separate Staff with UsersManage.
        using var client = factory.CreateClientAs(AccountType.Staff, userId: Guid.NewGuid(), companyId: company.Id, permissions: [Permissions.UsersManage]);

        var response = await client.DeleteAsync($"/api/users/{target.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteUser_self_delete_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        var self = await factory.SeedUserAsync(AccountType.Staff, "me@x.local", "Pass1234!", companyId: company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, userId: self.Id, companyId: company.Id, permissions: [Permissions.UsersManage]);

        var response = await client.DeleteAsync($"/api/users/{self.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DeleteUser_last_Owner_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        var ownerRole = await factory.SeedRoleAsync(SystemRoles.Owner, company.Id);
        var lastOwner = await factory.SeedUserAsync(AccountType.Staff, "owner@x.local", "Pass1234!", companyId: company.Id);
        await factory.AssignRoleAsync(lastOwner.Id, ownerRole.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, userId: Guid.NewGuid(), companyId: company.Id, permissions: [Permissions.UsersManage]);

        var response = await client.DeleteAsync($"/api/users/{lastOwner.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DeleteUser_other_company_returns_403()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        var foreign = await factory.SeedUserAsync(AccountType.Staff, "foreign@x.local", "Pass1234!", companyId: otherCompany.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, userId: Guid.NewGuid(), companyId: ownCompany.Id, permissions: [Permissions.UsersManage]);

        var response = await client.DeleteAsync($"/api/users/{foreign.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteUser_without_UsersManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var target = await factory.SeedUserAsync(AccountType.Staff, "target@x.local", "Pass1234!", companyId: company.Id);
        using var client = factory.CreateClientAs(AccountType.Staff, userId: Guid.NewGuid(), companyId: company.Id, permissions: []);

        var response = await client.DeleteAsync($"/api/users/{target.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
