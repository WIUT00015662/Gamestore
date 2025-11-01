using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gamestore.BLL.Serialization;

public class NullableGuidEmptyStringJsonConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (Guid.TryParse(value, out var guid))
            {
                return guid;
            }
        }

        throw new JsonException("The JSON value could not be converted to nullable Guid.");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
            return;
        }

        writer.WriteNullValue();
    }
}
