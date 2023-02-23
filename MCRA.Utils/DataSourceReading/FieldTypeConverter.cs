namespace MCRA.Utils.DataFileReading {

    public static class FieldTypeConverter {

        private static readonly Dictionary<string, FieldType> _typeStringMappings = 
            new(StringComparer.OrdinalIgnoreCase)
        {
            { "String", FieldType.AlphaNumeric },
            { "Byte", FieldType.Integer },
            { "Int", FieldType.Integer },
            { "Int16", FieldType.Integer },
            { "Int32", FieldType.Integer },
            { "Int64", FieldType.Integer },
            { "Integer", FieldType.Integer },
            { "Double", FieldType.Numeric },
            { "Float", FieldType.Numeric },
            { "Decimal", FieldType.Numeric },
            { "Boolean", FieldType.Boolean },
            { "Bool", FieldType.Boolean },
            { "DateTime", FieldType.DateTime }
        };

        /// <summary>
        /// Converts a string to a field type.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static FieldType FromSystemTypeString(string str) {
            if (_typeStringMappings.TryGetValue(str, out var fieldType)) {
                return fieldType;
            }
            return FieldType.Undefined;
        }

        /// <summary>
        /// Returns the field type belonging to the specified system type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldType FromSystemType(Type type) {
            if (type == typeof(int) || type == typeof(int?)) {
                return FieldType.Integer;
            } else if (type == typeof(short) || type == typeof(short?)) {
                return FieldType.Integer;
            } else if (type == typeof(byte) || type == typeof(byte?)) {
                return FieldType.Integer;
            } else if (type == typeof(long) || type == typeof(long?)) {
                return FieldType.Integer;
            } else if (type == typeof(double) || type == typeof(double?)) {
                return FieldType.Numeric;
            } else if (type == typeof(float) || type == typeof(float?)) {
                return FieldType.Numeric;
            } else if (type == typeof(bool) || type == typeof(bool?)) {
                return FieldType.Boolean;
            } else if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                return FieldType.DateTime;
            } else if (type == typeof(string)) {
                return FieldType.AlphaNumeric;
            } else {
                throw new Exception($"No field type mapping for type {type}.");
            }
        }

        /// <summary>
        /// Return preferred system type for the field type
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static Type ToSystemType(FieldType fieldType) {
            switch (fieldType) {
                case FieldType.Numeric:
                    return typeof(double);
                case FieldType.Boolean:
                    return typeof(bool);
                case FieldType.Integer:
                    return typeof(int);
                case FieldType.DateTime:
                    return typeof(DateTime);
                default:
                    return typeof(string);
            }
        }
    }
}
