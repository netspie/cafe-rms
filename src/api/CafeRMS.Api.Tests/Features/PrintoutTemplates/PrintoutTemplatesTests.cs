using System.Net;
using System.Net.Http.Json;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PrintoutTemplates;
using CafeRMS.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CafeRMS.Api.Tests.Features.PrintoutTemplates;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class PrintoutTemplatesTests : IDisposable
{
    private readonly ApiFactory factory = new();

    public void Dispose() => factory.Dispose();

    private sealed record TemplateDetail(Guid Id, string Name, string TemplateFileUrl, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
    private sealed record TemplateItem(Guid Id, string Name, string TemplateFileUrl, DateTimeOffset CreatedAt);
    private sealed record PageDto(IReadOnlyList<TemplateItem> Items, int Page, int PageSize, int Total);

    [Test]
    public async Task Add_happy_path_returns_id()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.PostAsJsonAsync("/api/printout-templates", new
        {
            name = "Receipt",
            templateFileUrl = "https://storage/receipt.docx"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Add_duplicate_name_returns_409()
    {
        await SeedTemplateAsync("Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.PostAsJsonAsync("/api/printout-templates", new
        {
            name = "Receipt",
            templateFileUrl = "https://storage/r2.docx"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Add_without_permission_returns_403()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: []);

        var response = await client.PostAsJsonAsync("/api/printout-templates", new
        {
            name = "Receipt",
            templateFileUrl = "https://storage/r.docx"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetById_happy_path_returns_template()
    {
        var id = await SeedTemplateAsync("Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.GetAsync($"/api/printout-templates/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TemplateDetail>();
        body!.Name.Should().Be("Receipt");
    }

    [Test]
    public async Task GetById_returns_404_when_missing()
    {
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.GetAsync($"/api/printout-templates/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var id = await SeedTemplateAsync("Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.PutAsJsonAsync($"/api/printout-templates/{id}", new
        {
            name = "Receipt v2",
            templateFileUrl = "https://storage/receipt-v2.docx"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Delete_happy_path_returns_204()
    {
        var id = await SeedTemplateAsync("Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.DeleteAsync($"/api/printout-templates/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<Guid> SeedTemplateAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var template = PrintoutTemplate.Create(name, "https://storage/" + name + ".docx");
        db.PrintoutTemplates.Add(template);
        await db.SaveChangesAsync();
        return template.Id;
    }
}
