using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcanaLedger.Server.Dtos.Json;

/// <summary>
/// Serializes <see cref="DateTimeOffset"/> values as ISO-8601 UTC strings with a trailing
/// <c>Z</c> (e.g. <c>"2026-09-29T14:03:00Z"</c>), matching the <c>timestamptz</c> convention
/// in <c>docs/api.md</c>.
/// </summary>
public sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss'Z'";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTimeOffset();

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.UtcDateTime.ToString(Format, CultureInfo.InvariantCulture));
}

/// <summary>
/// Nullable counterpart of <see cref="UtcDateTimeOffsetJsonConverter"/>.
/// </summary>
public sealed class NullableUtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    private static readonly UtcDateTimeOffsetJsonConverter Inner = new();

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : Inner.Read(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Inner.Write(writer, value.Value, options);
    }
}
