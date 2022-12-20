using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCRA.Utils.Converters {
    /// <summary>
    /// For an array of double values, read as string and convert
    /// to double using custom format
    /// When a null value is encountered it is not added to the array
    /// </summary>
    public class DoubleArrayJsonConverter : JsonConverter<double[]> {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(double[]);

        public override double[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            bool isArray = reader.TokenType == JsonTokenType.StartArray;
            var result = new List<double>();

            if(isArray) {
                reader.Read();
                while (reader.TokenType != JsonTokenType.EndArray) {
                    //we MAY encounter a nested Array: in this case
                    //read all values of the nested arrays into the returned flat array
                    if (reader.TokenType == JsonTokenType.StartArray) {
                        reader.Read(); //read next token
                        while (reader.TokenType != JsonTokenType.EndArray) {
                            var doubleValue = reader.ReadSingleDoubleOrNullValue();
                            if (doubleValue.HasValue) {
                                result.Add(doubleValue.Value);
                            }
                            reader.Read();
                        }
                    } else {
                        var doubleValue = reader.ReadSingleDoubleOrNullValue();
                        if (doubleValue.HasValue) {
                            result.Add(doubleValue.Value);
                        }
                    }
                    reader.Read();
                }
            } else {
                //read single value, skip when null
                var doubleValue = reader.ReadSingleDoubleOrNullValue();
                if (doubleValue.HasValue) {
                    result.Add(doubleValue.Value);
                }
            }

            return result.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, double[] value, JsonSerializerOptions options) {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
