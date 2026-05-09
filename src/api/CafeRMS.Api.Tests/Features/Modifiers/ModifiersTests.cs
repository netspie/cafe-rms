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

    private sealed record ModifierDetail(Guid Id, string Name, Guid ModifierGroupId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record ModifierItem(Guid Id, string Name, Guid ModifierGroupId, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<ModifierItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        var groupId = await SeedGroupAsync("Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = groupId, name = "Oat milk" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_with_unknown_group_returns_404()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = Guid.NewGuid(), name = "Oat milk" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Add_duplicate_name_in_same_group_returns_409()
    {
        var groupId = await SeedGroupAsync("Milk type");
        await SeedModifierAsync(groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = groupId, name = "Oat milk" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_same_name_in_different_groups_is_allowed()
    {
        var sizeGroup = await SeedGroupAsync("Size");
        var cupSizeGroup = await SeedGroupAsync("Cup size");
        await SeedModifierAsync(sizeGroup, "Small");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = cupSizeGroup, name = "Small" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_without_ModifiersManage_returns_403()
    {
        var groupId = await SeedGroupAsync("Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/modifiers", new { modifierGroupId = groupId, name = "Oat milk" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_modifier()
    {
        var groupId = await SeedGroupAsync("Milk type");
        var id = await SeedModifierAsync(groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.GetAsync($"/api/modifiers/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ModifierDetail>();
        body!.Name.Should().Be("Oat milk");
        body.ModifierGroupId.Should().Be(groupId);
    }

    [Test]
    public async Task List_filter_by_group_returns_only_matches()
    {
        var sizeGroup = await SeedGroupAsync("Size");
        var milkGroup = await SeedGroupAsync("Milk");
        await SeedModifierAsync(sizeGroup, "Small");
        await SeedModifierAsync(sizeGroup, "Large");
        await SeedModifierAsync(milkGroup, "Oat");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.GetAsync($"/api/modifiers?modifierGroupId={sizeGroup}");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Total.Should().Be(2);
        body.Items.Select(x => x.Name).Should().BeEquivalentTo(["Small", "Large"]);
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var groupId = await SeedGroupAsync("Milk type");
        var id = await SeedModifierAsync(groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PutAsJsonAsync($"/api/modifiers/{id}", new { name = "Oat milk (organic)" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_happy_path_returns_204()
    {
        var groupId = await SeedGroupAsync("Milk type");
        var id = await SeedModifierAsync(groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.DeleteAsync($"/api/modifiers/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.DeleteAsync($"/api/modifiers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedGroupAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var group = ModifierGroup.Create(name);
        db.ModifierGroups.Add(group);
        await db.SaveChangesAsync();
        return group.Id;
    }

    private async Task<Guid> SeedModifierAsync(Guid groupId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var modifier = Modifier.Create(name, groupId);
        db.Modifiers.Add(modifier);
        await db.SaveChangesAsync();
        return modifier.Id;
    }
}
