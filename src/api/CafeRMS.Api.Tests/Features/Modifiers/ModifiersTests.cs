using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Features.Modifiers;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.Modifiers;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ModifiersTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record ModifierDetail(Guid Id, string Name, decimal PriceDelta, Guid ModifierGroupId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record ModifierItem(Guid Id, string Name, decimal PriceDelta, Guid ModifierGroupId, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<ModifierItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        var company = await factory.SeedCompanyAsync();
        var groupId = await SeedGroupAsync(company.Id, "Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = groupId, name = "Oat milk", priceDelta = 0.5m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_with_unknown_group_returns_404()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = Guid.NewGuid(), name = "Oat milk", priceDelta = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Add_duplicate_name_in_same_group_returns_409()
    {
        var company = await factory.SeedCompanyAsync();
        var groupId = await SeedGroupAsync(company.Id, "Milk type");
        await SeedModifierAsync(company.Id, groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = groupId, name = "Oat milk", priceDelta = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_same_name_in_different_groups_is_allowed()
    {
        var company = await factory.SeedCompanyAsync();
        var sizeGroup = await SeedGroupAsync(company.Id, "Size");
        var cupSizeGroup = await SeedGroupAsync(company.Id, "Cup size");
        await SeedModifierAsync(company.Id, sizeGroup, "Small");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = cupSizeGroup, name = "Small", priceDelta = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_without_ModifiersManage_returns_403()
    {
        var company = await factory.SeedCompanyAsync();
        var groupId = await SeedGroupAsync(company.Id, "Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = groupId, name = "Oat milk", priceDelta = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_modifier_with_priceDelta()
    {
        var company = await factory.SeedCompanyAsync();
        var groupId = await SeedGroupAsync(company.Id, "Milk type");
        var id = await SeedModifierAsync(company.Id, groupId, "Oat milk", priceDelta: 0.5m);
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.GetAsync($"/api/modifiers/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ModifierDetail>();
        body!.Name.Should().Be("Oat milk");
        body.PriceDelta.Should().Be(0.5m);
    }

    [Test]
    public async Task List_filter_by_group_returns_only_matches()
    {
        var company = await factory.SeedCompanyAsync();
        var sizeGroup = await SeedGroupAsync(company.Id, "Size");
        var milkGroup = await SeedGroupAsync(company.Id, "Milk");
        await SeedModifierAsync(company.Id, sizeGroup, "Small");
        await SeedModifierAsync(company.Id, sizeGroup, "Large");
        await SeedModifierAsync(company.Id, milkGroup, "Oat");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.GetAsync($"/api/modifiers?modifierGroupId={sizeGroup}");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(2);
        body.Items.Select(x => x.Name).Should().BeEquivalentTo(["Small", "Large"]);
    }

    [Test]
    public async Task List_does_not_include_other_companies_items()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        var ownGroup = await SeedGroupAsync(ownCompany.Id, "Own");
        var otherGroup = await SeedGroupAsync(otherCompany.Id, "Other");
        await SeedModifierAsync(ownCompany.Id, ownGroup, "OurMod");
        await SeedModifierAsync(otherCompany.Id, otherGroup, "TheirMod");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.GetAsync("/api/modifiers");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurMod");
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var groupId = await SeedGroupAsync(company.Id, "Milk type");
        var id = await SeedModifierAsync(company.Id, groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.PutAsJsonAsync($"/api/modifiers/{id}", new { name = "Oat milk (organic)", priceDelta = 0.75m });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var groupId = await SeedGroupAsync(company.Id, "Milk type");
        var id = await SeedModifierAsync(company.Id, groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.DeleteAsync($"/api/modifiers/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.ModifiersManage]);

        var response = await client.DeleteAsync($"/api/modifiers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedGroupAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var group = ModifierGroup.Create(name, companyId);
        db.ModifierGroups.Add(group);
        await db.SaveChangesAsync();
        return group.Id;
    }

    private async Task<Guid> SeedModifierAsync(Guid companyId, Guid groupId, string name, decimal priceDelta = 0m)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var modifier = Modifier.Create(name, groupId, companyId, priceDelta);
        db.Modifiers.Add(modifier);
        await db.SaveChangesAsync();
        return modifier.Id;
    }
}
