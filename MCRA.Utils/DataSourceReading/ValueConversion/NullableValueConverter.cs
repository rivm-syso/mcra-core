namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Wrapper of <see cref="IValueConverter"/> for nullable types.
    /// <see cref="long"/> values.
    /// </summary>
    public class NullableValueConverter<T> : IValueConverter where T : IValueConverter {

        private readonly T _valueConverter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="valueConverter"></param>
        public NullableValueConverter(T valueConverter) {
            _valueConverter = valueConverter;
        }

        /// <summary>
        /// Converts a <see cref="string"/> value to the nullable value type.
        /// If the string is null or empty, null is returned.
        /// </summary>
        public object Convert(string value) {
            if (string.IsNullOrEmpty(value)) {
                return null;
            } else {
                return _valueConverter.Convert(value);
            }
        }
    }
}
