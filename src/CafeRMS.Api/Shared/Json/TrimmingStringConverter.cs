using System.Text.Json;
using System.Text.Json.Serialization;

namespace CafeRMS.Api.Shared.Json;

// Trims leading/trailing whitespace from every JSON string deserialized into a request DTO.
// Treats surrounding whitespace as transport noise so use cases never see padded input.
public sealed class TrimmingStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString()?.Trim();

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
