using System.Text.Json;

namespace MCRA.Utils.Converters {
    public static class Utf8JsonReaderExtensions {
        public static double? ReadSingleDoubleOrNullValue(this Utf8JsonReader reader) {
            var result = (double?)null;
            if (reader.TokenType == JsonTokenType.Number) {
                result = reader.GetDouble();
            } else if (reader.TokenType == JsonTokenType.String) {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue) || stringValue == "NA") {
                    result = double.NaN;
                } else if (stringValue == "Inf") {
                    result = double.PositiveInfinity;
                } else if (stringValue == "-Inf") {
                    result = double.NegativeInfinity;
                }
            }
            return result;
        }
    }
}
