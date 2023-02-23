namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting <see cref="string"/>
    /// values to <see cref="object"/> values of other types using a custom parser
    /// function.
    /// </summary>
    public class CustomValueConverter : IValueConverter {

        private readonly Func<string, object> _parseFct;

        /// <summary>
        /// Creates a new <see cref="CustomValueConverter"/> instance.
        /// </summary>
        public CustomValueConverter(Func<string, object> parseFct) {
            _parseFct = parseFct;
        }

        /// <summary>
        /// Converts a <see cref="object"/> from a <see cref="string"/> value
        /// using the parse function.
        /// </summary>
        public object Convert(string value) {
            var result = _parseFct(value);
            return result;
        }
    }
}
