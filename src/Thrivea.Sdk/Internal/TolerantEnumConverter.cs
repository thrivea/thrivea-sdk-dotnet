using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Thrivea.Internal;

/// <summary>
///     Reads an enum the API writes as a number today and may write as a name tomorrow; a value this version does not
///     know becomes the enum's zero member rather than an exception, as the API's evolution rules ask of clients.
/// </summary>
internal sealed class TolerantEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number when reader.TryGetInt32(out var number):
                return (TEnum)Enum.ToObject(typeof(TEnum), number);
            case JsonTokenType.String:
                var text = reader.GetString();
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    return (TEnum)Enum.ToObject(typeof(TEnum), parsed);
                }

                return Enum.TryParse<TEnum>(text?.Replace("_", string.Empty), ignoreCase: true, out var named) ? named : default;
            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value, CultureInfo.InvariantCulture));
    }
}
