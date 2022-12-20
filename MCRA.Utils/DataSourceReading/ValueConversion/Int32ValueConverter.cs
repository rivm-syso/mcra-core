using System.Globalization;

namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to 
    /// <see cref="int"/> values.
    /// </summary>
    public class Int32ValueConverter : IValueConverter {

        /// <summary>
        /// Culture info used for parsing the value. Default is invariant culture.
        /// </summary>
        public readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        /// <summary>
        /// Converts a <see cref="int"/> from a <see cref="string"/> value.
        /// </summary>
        public object Convert(string value) {
            var result = int.Parse(value, CultureInfo);
            return result;
        }
    }
}
