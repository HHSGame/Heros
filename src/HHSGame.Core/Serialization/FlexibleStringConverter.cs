using System.Text.Json;
using System.Text.Json.Serialization;

namespace HHSGame.Core.Serialization
{
    /// <summary>
    /// 灵活的字符串转换器，支持数字和字符串格式
    /// </summary>
    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32().ToString();
            }
            else if (reader.TokenType == JsonTokenType.True)
            {
                return "true";
            }
            else if (reader.TokenType == JsonTokenType.False)
            {
                return "false";
            }

            return string.Empty;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
