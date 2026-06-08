using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.PrintoutTemplates;

public class PrintoutTemplate : SoftDeletableEntity
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = "";
    public string FileName { get; private set; } = "";
    public string ContentType { get; private set; } = "";
    public byte[] FileContent { get; private set; } = [];

    private PrintoutTemplate() { }

    public static PrintoutTemplate Create(string name, string fileName, string contentType, byte[] fileContent)
    {
        return new PrintoutTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            FileName = fileName,
            ContentType = contentType,
            FileContent = fileContent
        };
    }

    public void Update(string name, string fileName, string contentType, byte[] fileContent)
    {
        Name = name;
        FileName = fileName;
        ContentType = contentType;
        FileContent = fileContent;
    }
}
