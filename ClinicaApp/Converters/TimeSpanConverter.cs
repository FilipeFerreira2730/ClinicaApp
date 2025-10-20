using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClinicaApp.Converters
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        private const string Format = @"hh\:mm";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return TimeSpan.Zero;

            // Aceita tanto "HH:mm" como "HH:mm:ss"
            if (TimeSpan.TryParse(value, out var time))
                return time;

            throw new JsonException($"Valor inválido para TimeSpan: {value}");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
