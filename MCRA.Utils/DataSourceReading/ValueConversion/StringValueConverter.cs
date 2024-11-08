namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to
    /// <see cref="bool"/> values.
    /// </summary>
    public class StringValueConverter : IValueConverter {

        /// <summary>
        /// Converts a <see cref="string"/> value to a <see cref="string"/> value.
        /// </summary>
        public object Convert(string value) {
            var result = value;
            return result;
        }
    }
}
