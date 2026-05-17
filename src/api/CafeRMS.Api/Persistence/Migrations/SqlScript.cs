using System.Reflection;

namespace CafeRMS.Api.Persistence.Migrations;

internal static class SqlScript
{
    public static string Load(string fileName)
    {
        var assembly = typeof(SqlScript).Assembly;
        var resourceName = $"CafeRMS.Api.Persistence.Migrations.Sql.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"SQL resource not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
