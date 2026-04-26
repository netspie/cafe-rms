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
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PrintoutTemplatesManage]);

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
        var company = await factory.SeedCompanyAsync();
        await SeedTemplateAsync(company.Id, "Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PrintoutTemplatesManage]);

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
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: []);

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
        var company = await factory.SeedCompanyAsync();
        var id = await SeedTemplateAsync(company.Id, "Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.GetAsync($"/api/printout-templates/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TemplateDetail>();
        body!.Name.Should().Be("Receipt");
    }

    [Test]
    public async Task GetById_returns_404_when_missing()
    {
        var company = await factory.SeedCompanyAsync();
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.GetAsync($"/api/printout-templates/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task List_does_not_include_other_companies_items()
    {
        var ownCompany = await factory.SeedCompanyAsync(legalName: "Own", taxId: "1");
        var otherCompany = await factory.SeedCompanyAsync(legalName: "Other", taxId: "2");
        await SeedTemplateAsync(ownCompany.Id, "OurReceipt");
        await SeedTemplateAsync(otherCompany.Id, "TheirReceipt");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: ownCompany.Id, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.GetAsync("/api/printout-templates");

        var body = await response.Content.ReadFromJsonAsync<PageDto>();
        body!.Items.Select(x => x.Name).Should().Equal("OurReceipt");
    }

    [Test]
    public async Task Update_happy_path_returns_204()
    {
        var company = await factory.SeedCompanyAsync();
        var id = await SeedTemplateAsync(company.Id, "Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PrintoutTemplatesManage]);

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
        var company = await factory.SeedCompanyAsync();
        var id = await SeedTemplateAsync(company.Id, "Receipt");
        using var client = factory.CreateClientAs(AccountType.Staff, companyId: company.Id, permissions: [Permissions.PrintoutTemplatesManage]);

        var response = await client.DeleteAsync($"/api/printout-templates/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<Guid> SeedTemplateAsync(Guid companyId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var template = PrintoutTemplate.Create(name, "https://storage/" + name + ".docx", companyId);
        db.PrintoutTemplates.Add(template);
        await db.SaveChangesAsync();
        return template.Id;
    }
}
