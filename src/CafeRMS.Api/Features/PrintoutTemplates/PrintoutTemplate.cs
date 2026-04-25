using CafeRMS.Api.Features.Companies;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.PrintoutTemplates;

public class PrintoutTemplate : IAuditable, ISoftDeletable, ICompanyScoped
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string TemplateFileUrl { get; private set; } = "";
    public Guid CompanyId { get; private init; }
    public Company? Company { get; private init; }

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

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
