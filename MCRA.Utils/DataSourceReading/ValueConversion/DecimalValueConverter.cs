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
            var result = decimal.Parse(value, CultureInfo);
            return result;
        }
    }
}
