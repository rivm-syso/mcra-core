namespace MCRA.Utils.DataSourceReading.ValueConversion {
    public class ValueConverterCollection {

        private readonly Dictionary<Type, IValueConverter> _valueConverters;

        /// <summary>
        /// Creates a new <see cref="ValueConverterCollection"/> instance.
        /// </summary>
        public ValueConverterCollection() {
            _valueConverters = new Dictionary<Type, IValueConverter>();
        }

        /// <summary>
        /// Registers the specified <see cref="IValueConverter"/> to be used
        /// for the specified value type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valueConverter"></param>
        public void Add(Type type, IValueConverter valueConverter) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }
            if (valueConverter == null) {
                throw new ArgumentNullException(nameof(valueConverter));
            }
            _valueConverters[type] = valueConverter;
        }

        /// <summary>
        /// Gets the converter for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type for which we need the converter.</param>
        /// <returns>The <see cref="IValueConverter"/> for the specified <see cref="Type"/>.</returns>
        public IValueConverter Get(Type type) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }
            if (_valueConverters.TryGetValue(type, out var valueConverter)) {
                return valueConverter;
            }
            throw new Exception($"No value converter found for type {type}.");
        }

        /// <summary>
        /// Converts the <see cref="string"/> value to a value of the specified generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert<T>(string value) {
            return Convert(value, typeof(T));
        }

        /// <summary>
        /// Converts the <see cref="object"/> value to a value of the specified type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Convert(object value, Type type) {
            if (value.GetType() == typeof(string)) {
                var converter = Get(type);
                return converter.Convert((string)value);
            } else if (value.GetType() == type) {
                return value;
            } else {
                throw new Exception($"Cannot convert value of type {value.GetType()} to {type}.");
            }
        }

        /// <summary>
        /// Converts the <see cref="string"/> value to a value of the specified type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert(string value, Type type) {
            var converter = Get(type);
            var result = converter.Convert(value);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="ValueConverterCollection"/> instance containing 
        /// value converter for the most common types.
        /// </summary>
        /// <returns></returns>
        public static ValueConverterCollection Default() {
            var result = new ValueConverterCollection();
            result.Add(typeof(bool), new BoolValueConverter());
            result.Add(typeof(DateTime), new DateTimeValueConverter());
            result.Add(typeof(DateTime?), new NullableValueConverter<DateTimeValueConverter>(new DateTimeValueConverter()));
            result.Add(typeof(decimal), new DecimalValueConverter());
            result.Add(typeof(decimal?), new NullableValueConverter<DecimalValueConverter>(new DecimalValueConverter()));
            result.Add(typeof(double), new DoubleValueConverter());
            result.Add(typeof(double?), new NullableValueConverter<DoubleValueConverter>(new DoubleValueConverter()));
            result.Add(typeof(float), new FloatValueConverter());
            result.Add(typeof(float?), new NullableValueConverter<FloatValueConverter>(new FloatValueConverter()));
            result.Add(typeof(short), new Int16ValueConverter());
            result.Add(typeof(short?), new NullableValueConverter<Int16ValueConverter>(new Int16ValueConverter()));
            result.Add(typeof(int), new Int32ValueConverter());
            result.Add(typeof(int?), new NullableValueConverter<Int32ValueConverter>(new Int32ValueConverter()));
            result.Add(typeof(long), new Int64ValueConverter());
            result.Add(typeof(long?), new NullableValueConverter<Int64ValueConverter>(new Int64ValueConverter()));
            result.Add(typeof(string), new StringValueConverter());
            return result;
        }
    }
}
