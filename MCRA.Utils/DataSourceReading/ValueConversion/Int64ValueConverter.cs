using System.Globalization;

namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to 
    /// <see cref="long"/> values.
    /// </summary>
    public class Int64ValueConverter : IValueConverter {

        /// <summary>
        /// Culture info used for parsing the value. Default is invariant culture.
        /// </summary>
        public readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        /// <summary>
        /// Converts a <see cref="long"/> from a <see cref="string"/> value.
        /// </summary>
        public object Convert(string value) {
            var result = long.Parse(value, CultureInfo);
            return result;
        }
    }
}
