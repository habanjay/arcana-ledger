using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcanaLedger.Server.Dtos.Json;

/// <summary>
/// Serializes <c>numeric(20, 8)</c>-backed fields (prices, quantities, cash amounts) as
/// fixed-precision JSON strings (e.g. <c>"182.50000000"</c>) instead of floating point
/// numbers, matching the money/quantity convention in <c>docs/api.md</c>.
/// </summary>
public sealed class MoneyJsonConverter : JsonConverter<decimal>
{
    private const string Format = "F8";

    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return decimal.Parse(value ?? string.Empty, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}

/// <summary>
/// Nullable counterpart of <see cref="MoneyJsonConverter"/> for optional
/// <c>numeric(20, 8)</c>-backed fields.
/// </summary>
public sealed class NullableMoneyJsonConverter : JsonConverter<decimal?>
{
    private static readonly MoneyJsonConverter Inner = new();

    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : Inner.Read(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Inner.Write(writer, value.Value, options);
    }
}
