using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.PrintoutTemplates;

public class PrintoutTemplate : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string TemplateFileUrl { get; private set; } = "";

    private PrintoutTemplate() { }

    public static PrintoutTemplate Create(string name, string templateFileUrl)
    {
        return new PrintoutTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            TemplateFileUrl = templateFileUrl
        };
    }

    public void Update(string name, string templateFileUrl)
    {
        Name = name;
        TemplateFileUrl = templateFileUrl;
    }
}
