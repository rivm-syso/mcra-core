using System;
using System.Collections.Generic;
using System.Globalization;

namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to 
    /// <see cref="float"/> values.
    /// </summary>
    public class FloatValueConverter : IValueConverter {

        /// <summary>
        /// Special strings representing NaN values. Default: "NA" and "NaN".
        /// </summary>
        public readonly HashSet<string> NaNStrings 
            = new(StringComparer.OrdinalIgnoreCase) { "NA", "NaN" };

        /// <summary>
        /// Culture info used for parsing the value. Default is invariant culture.
        /// </summary>
        public readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        /// <summary>
        /// Converts a <see cref="float"/> from a <see cref="string"/> value.
        /// </summary>
        public object Convert(string value) {
            var result = !NaNStrings.Contains(value)
                ? float.Parse(value, CultureInfo)
                : float.NaN;
            return result;
        }
    }
}
