using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCRA.Utils.Converters {
    /// <summary>
    /// For an array of double values, read as string and convert
    /// to double using custom format
    /// </summary>
    public class DoubleMatrixJsonConverter : JsonConverter<double[][]> {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(double[][]);

        public override double[][] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            var result = new List<double[]>();

            //read rest of array but don't process
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndArray) {
                if (reader.TokenType == JsonTokenType.StartArray) {
                    var arr = new List<double>();
                    reader.Read(); //read next token
                    while (reader.TokenType != JsonTokenType.EndArray) {
                        var doubleValue = reader.ReadSingleDoubleOrNullValue();
                        arr.Add(doubleValue ?? double.NaN);
                        reader.Read();
                    }
                    result.Add(arr.ToArray());
                }
                reader.Read();
            }

            return result.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, double[][] value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            foreach(var va in value) {
                writer.WriteStartArray();
                foreach(var v in va) {
                    JsonSerializer.Serialize(writer, v, options);
                }
                writer.WriteEndArray();
            }
            JsonSerializer.Serialize(writer, value, options);
            writer.WriteEndArray();
        }
    }
}
