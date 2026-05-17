using System.Globalization;
using System.IO.Compression;
using System.Text;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PrintoutTemplates;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class GenerateEventConfirmationController : ControllerBase
{
    [HttpGet("/api/events/{id:guid}/confirmation")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        var result = await GenerateEventConfirmation.Execute(id, db);
        return File(result.Bytes, GenerateEventConfirmation.DocxContentType, result.FileName);
    }
}

public static class GenerateEventConfirmation
{
    public const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    private const string TemplateName = "Potwierdzenie wydarzenia";
    private const string DocumentEntryPath = "word/document.xml";

    public sealed record Result(byte[] Bytes, string FileName);

    public static async Task<Result> Execute(Guid eventId, AppDbContext db)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == eventId)
            ?? throw new NotFoundException("Event not found.");

        var days = await db.EventDays
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.Date)
            .Select(x => x.Date)
            .ToListAsync();

        var template = await db.PrintoutTemplates.FirstOrDefaultAsync(x => x.Name == TemplateName)
            ?? throw new NotFoundException($"Printout template '{TemplateName}' not found.");

        var templatePath = ResolveTemplatePath(template.TemplateFileUrl);
        var replacements = BuildReplacements(ev, days);
        var bytes = MergeDocx(templatePath, replacements);

        var safeName = SanitizeFileName(ev.Name);
        var fileName = $"potwierdzenie-{safeName}.docx";
        return new Result(bytes, fileName);
    }

    private static string ResolveTemplatePath(string templateFileUrl)
    {
        var fileName = Path.GetFileName(templateFileUrl);
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "Templates", fileName);
        if (!File.Exists(path))
            throw new InvalidOperationException($"Template file not found on disk: {path}");
        return path;
    }

    private static Dictionary<string, string> BuildReplacements(Event ev, IReadOnlyList<DateOnly> days)
    {
        var pl = CultureInfo.GetCultureInfo("pl-PL");
        return new Dictionary<string, string>
        {
            ["{{event_name}}"]         = ev.Name,
            ["{{event_description}}"]  = ev.Description ?? "—",
            ["{{event_status}}"]       = TranslateStatus(ev.Status),
            ["{{event_days_count}}"]   = days.Count.ToString(pl),
            ["{{event_first_day}}"]    = days.Count > 0 ? days[0].ToString("dd.MM.yyyy", pl) : "—",
            ["{{event_last_day}}"]     = days.Count > 0 ? days[^1].ToString("dd.MM.yyyy", pl) : "—",
            ["{{event_published_at}}"] = ev.PublishedAt is { } published
                ? published.ToLocalTime().ToString("dd.MM.yyyy HH:mm", pl)
                : "—",
            ["{{event_id}}"]           = ev.Id.ToString(),
            ["{{generated_at}}"]       = DateTimeOffset.UtcNow.ToLocalTime().ToString("dd.MM.yyyy HH:mm", pl)
        };
    }

    private static string TranslateStatus(EventStatus status) => status switch
    {
        EventStatus.Draft     => "Szkic",
        EventStatus.Published => "Opublikowane",
        EventStatus.Closed    => "Zakończone",
        EventStatus.Cancelled => "Anulowane",
        _                     => status.ToString()
    };

    private static byte[] MergeDocx(string templatePath, IReadOnlyDictionary<string, string> replacements)
    {
        var templateBytes = File.ReadAllBytes(templatePath);
        using var outStream = new MemoryStream();
        outStream.Write(templateBytes, 0, templateBytes.Length);
        outStream.Position = 0;

        using (var zip = new ZipArchive(outStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            var entry = zip.GetEntry(DocumentEntryPath)
                ?? throw new InvalidOperationException($"Template is malformed — missing {DocumentEntryPath}.");

            string documentXml;
            using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                documentXml = reader.ReadToEnd();

            foreach (var (token, value) in replacements)
                documentXml = documentXml.Replace(token, EscapeXml(value));

            entry.Delete();
            var fresh = zip.CreateEntry(DocumentEntryPath, CompressionLevel.Optimal);
            using var writer = new StreamWriter(fresh.Open(), new UTF8Encoding(false));
            writer.Write(documentXml);
        }

        return outStream.ToArray();
    }

    private static string EscapeXml(string value) =>
        value.Replace("&", "&amp;")
             .Replace("<", "&lt;")
             .Replace(">", "&gt;");

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new StringBuilder(name.Length);
        foreach (var ch in name)
            cleaned.Append(Array.IndexOf(invalid, ch) >= 0 || ch == ' ' ? '-' : ch);
        return cleaned.ToString().Trim('-').ToLowerInvariant();
    }
}
