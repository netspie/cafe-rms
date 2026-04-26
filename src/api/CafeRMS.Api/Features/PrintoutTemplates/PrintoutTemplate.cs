using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.PrintoutTemplates;

public class PrintoutTemplate : CompanyOwnedSoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string TemplateFileUrl { get; private set; } = "";

    private PrintoutTemplate() { }

    public static PrintoutTemplate Create(string name, string templateFileUrl, Guid companyId)
    {
        return new PrintoutTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            TemplateFileUrl = templateFileUrl,
            CompanyId = companyId
        };
    }
}
