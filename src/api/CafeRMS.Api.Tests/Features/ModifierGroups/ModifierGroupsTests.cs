using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ModifierGroups;
using CafeRMS.Api.Features.Modifiers;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.ModifierGroups;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ModifierGroupsTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record GroupDetail(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record GroupItem(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<GroupItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifier-groups", new { name = "Milk type" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_duplicate_name_returns_409()
    {
        await SeedGroupAsync("Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PostAsJsonAsync("/api/modifier-groups", new { name = "Milk type" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_without_ModifiersManage_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/modifier-groups", new { name = "Milk type" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_returns_group()
    {
        var id = await SeedGroupAsync("Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.GetAsync($"/api/modifier-groups/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GroupDetail>();
        body!.Name.Should().Be("Milk type");
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var id = await SeedGroupAsync("Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.PutAsJsonAsync($"/api/modifier-groups/{id}", new { name = "Milk options" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_happy_path_returns_204()
    {
        var id = await SeedGroupAsync("Milk type");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.DeleteAsync($"/api/modifier-groups/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_with_child_modifiers_returns_409()
    {
        var groupId = await SeedGroupAsync("Milk type");
        await SeedModifierAsync(groupId, "Oat milk");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.DeleteAsync($"/api/modifier-groups/{groupId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Delete_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.ModifiersManage]);

        var response = await client.DeleteAsync($"/api/modifier-groups/{Guid.NewGuid()}");

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

    private async Task SeedModifierAsync(Guid groupId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Modifiers.Add(Modifier.Create(name, groupId));
        await db.SaveChangesAsync();
    }
}
