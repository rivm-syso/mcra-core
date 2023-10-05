using System.Globalization;

namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to 
    /// <see cref="decimal"/> values.
    /// </summary>
    public class DecimalValueConverter : IValueConverter {

        /// <summary>
        /// Culture info used for parsing the value. Default is invariant culture.
        /// </summary>
        public readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        /// <summary>
        /// Converts a <see cref="decimal"/> from a <see cref="string"/> value.
        /// </summary>
        public object Convert(string value) {
            if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo, out var result)) {
                throw new FormatException($"String {value} is not recognized as a valid numeric value.");
            }
            return result;
        }
    }
}
