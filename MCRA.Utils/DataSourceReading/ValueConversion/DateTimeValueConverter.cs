using System.Globalization;

namespace MCRA.Utils.DataSourceReading.ValueConversion {


    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to 
    /// <see cref="DateTime"/> values.
    /// </summary>
    public class DateTimeValueConverter : IValueConverter {

        private static readonly string[] _defaultFormats = {
            "s", "u", "o", "r", "R",
            "d-M-yyyy", "d/M/yyyy",
            "d-M-yyyy H:m", "d/M/yyyy H:m",
            "d-M-yyyy H:m:s", "d/M/yyyy H:m:s",
            "yyyy-M-d", "yyyy/M/d",
            "yyyy-M-d H:m", "yyyy/M/d H:m",
            "yyyy-M-d H:m:s", "yyyy/M/d H:m:s",
        };

        private readonly string[] _formats;

        /// <summary>
        /// Creates a new <see cref="DateTimeValueConverter"/> instance.
        /// </summary>
        public DateTimeValueConverter(string[] formats = null) {
            _formats = formats ?? _defaultFormats;
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> from a <see cref="string"/> value.
        /// </summary>
        public object Convert(string value) {
            if (DateTime.TryParseExact(
                value.Replace("\"", "").Trim(),
                _formats,
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AllowInnerWhite, 
                out var result)
            ) {
                return result;
            } else {
                throw new FormatException($"Failed to parse '{value}' in column as a date/time value.");
            }
        }
    }
}
