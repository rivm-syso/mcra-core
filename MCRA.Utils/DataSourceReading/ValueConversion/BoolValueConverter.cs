namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to
    /// <see cref="bool"/> values.
    /// </summary>
    public class BoolValueConverter : IValueConverter {

        /// <summary>
        /// Converts a <see cref="bool"/> from a <see cref="string"/> value, which
        /// may be numeric (1 or 0 or "1" or "0"), or "True" or "False", to the
        /// boolean type.
        /// </summary>
        public object Convert(string value) {
            var result = bool.TryParse(value, out var boolValue)
                ? boolValue
                : System.Convert.ToInt32(value) != 0;
            return result;
        }
    }
}
