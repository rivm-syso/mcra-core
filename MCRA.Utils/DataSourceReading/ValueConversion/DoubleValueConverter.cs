using System.Globalization;

namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to
    /// <see cref="double"/> values.
    /// </summary>
    public class DoubleValueConverter : IValueConverter {

        /// <summary>
        /// Special strings representing NaN values. Default: "NA" and "NaN".
        /// </summary>
        public readonly HashSet<string> NaNStrings
            = new(StringComparer.OrdinalIgnoreCase) { "NA", "NaN", "-", string.Empty };

        /// <summary>
        /// Special strings representing infinity values. Default: "Inf".
        /// </summary>
        public readonly HashSet<string> InfStrings
            = new(StringComparer.OrdinalIgnoreCase) { "inf", "infinity" };

        /// <summary>
        /// Special strings representing negative infinity values. Default: "Inf".
        /// </summary>
        public readonly HashSet<string> NegativeInfStrings
            = new(StringComparer.OrdinalIgnoreCase) { "-inf", "-infinity" };

        /// <summary>
        /// Culture info used for parsing the value. Default is invariant culture.
        /// </summary>
        public readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        /// <summary>
        /// Converts a <see cref="double"/> from a <see cref="string"/> value.
        /// </summary>
        public object Convert(string value) {
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo, out var result)) {
                if (NaNStrings.Contains(value)) {
                    result = double.NaN;
                } else if (InfStrings.Contains(value)) {
                    result = double.PositiveInfinity;
                } else if (NegativeInfStrings.Contains(value)) {
                    result = double.NegativeInfinity;
                } else {
                    throw new FormatException($"String {value} is not recognized as a valid numeric value.");
                }
            }
            return result;
        }
    }
}
