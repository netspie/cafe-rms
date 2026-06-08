using CafeRMS.Api.Shared.Errors;

namespace CafeRMS.Api.Features.PrintoutTemplates;

public static class PrintoutFile
{
    public const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public static async Task<byte[]> ReadAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            throw new DomainException("A template file is required.");

        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Only .docx Word templates are supported.");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        return stream.ToArray();
    }
}
