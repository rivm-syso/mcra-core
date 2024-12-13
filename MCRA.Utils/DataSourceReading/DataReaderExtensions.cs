using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using MCRA.Utils.DataSourceReading.Attributes;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Utils.DataFileReading {
    public static class IDataReaderExtensions {

        /// <summary>
        /// Gets the column names / field names of the data reader.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static List<string> GetColumnNames(this IDataReader dataReader) {
            var result = new List<string>();
            for (int i = 0; i < dataReader.FieldCount; i++) {
                result.Add(dataReader.GetName(i));
            }
            return result;
        }

        /// <summary>
        /// Parses the field value of the current row as a double or null.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static double? GetDoubleOrNull(this IDataReader r, int? field) {
            if (field == null || r.IsDBNull((int)field)) {
                return null;
            }
            return r.GetDouble((int)field);
        }

        /// <summary>
        /// Reads the data table based on the provided target property mappings and
        /// returns a list of objects as determined by the generic object type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="tableDefinition"></param>
        public static IEnumerable<T> ReadRecords<T>(
            this IDataReader reader,
            TableDefinition tableDefinition
        ) where T : new() {
            if (reader != null) {
                var columnMappings = new Dictionary<int, string>();
                for (int i = 0; i < reader.FieldCount; i++) {
                    var columnName = reader.GetName(i);
                    var destination = tableDefinition.FindColumnDefinitionByAlias(columnName);
                    if (destination != null) {
                        columnMappings.Add(i, destination.Id);
                    }
                }
                var targetRecordType = typeof(T);
                var recordCounter = 1;
                while (reader.Read()) {
                    var record = new T();
                    try {
                        for (int i = 0; i < reader.FieldCount; i++) {
                            if (columnMappings.TryGetValue(i, out var targetPropertyName)) {
                                if (targetRecordType.GetProperty(targetPropertyName) != null) {
                                    try {
                                        var targetPropertyType = targetRecordType.GetProperty(targetPropertyName).PropertyType;
                                        var value = convertToType(reader, i, targetPropertyType);
                                        targetRecordType.GetProperty(targetPropertyName).SetValue(record, value, null);
                                    } catch (Exception) {
                                        throw new Exception($"failed to map value for field {targetPropertyName}.");
                                    }
                                }
                            }
                        }
                        recordCounter++;
                    } catch (Exception ex) {
                        throw new Exception($"Error reading record {recordCounter}: {ex.Message}");
                    }
                    yield return record;
                }
            }
        }

        private static object convertToType(IDataReader reader, int fieldIndex, Type conversionType) {
            var value = reader[fieldIndex];

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
                // It's a nullable type.
                if (value == null || value == DBNull.Value ||
                    value is string strVal && strVal.Length == 0
                ) {
                    // If it's null, it won't convert to the underlying type, but that's fine since nulls
                    // don't really have a type-so just return null.
                    return null;
                }

                // Determine what the underlying type and proceed with the underlying type.
                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }

            if (conversionType.BaseType == typeof(Enum)) {
                // Target type is an Enum. Parse the enum value.
                return Enum.Parse(conversionType, value.ToString(), true);
            }

            // Now that we've guaranteed conversionType is something Convert.ChangeType
            // can handle, pass the call on to Convert.ChangeType.
            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses all records of the reader into a collection of the specified
        /// generic objects. Field mappings and field types are defined by the
        /// specified generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<T> ReadRecords<T>(this IDataReader reader) where T : new() {
            // Get field mappings
            var headers = reader.GetColumnNames();
            var properties = typeof(T).GetProperties();
            var mappings = new Dictionary<int, int>();
            for (int i = 0; i < properties.Length; i++) {
                var index = findMatchingHeader(properties[i], headers);
                mappings.Add(i, index);
            }

            // Read all records
            var recordCounter = 1;
            while (reader.Read()) {
                var record = new T();
                try {
                    for (int i = 0; i < properties.Length; i++) {
                        if (mappings[i] >= 0) {
                            var property = properties[i];
                            try {
                                if (reader.IsDBNull(mappings[i])) {
                                    property.SetValue(record, null);
                                } else {
                                    var conversionType = property.PropertyType;
                                    if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
                                        // Determine what the underlying type and proceed with the underlying type.
                                        var nullableConverter = new NullableConverter(conversionType);
                                        conversionType = nullableConverter.UnderlyingType;
                                    }

                                    var value = Type.GetTypeCode(conversionType) switch {
                                        TypeCode.Boolean => reader.GetBoolean(mappings[i]),
                                        TypeCode.Char => reader.GetChar(mappings[i]),
                                        TypeCode.Byte => reader.GetByte(mappings[i]),
                                        TypeCode.Int16 => reader.GetInt16(mappings[i]),
                                        TypeCode.Int32 => reader.GetInt32(mappings[i]),
                                        TypeCode.Int64 => reader.GetInt64(mappings[i]),
                                        TypeCode.Single => reader.GetFloat(mappings[i]),
                                        TypeCode.Double => reader.GetDouble(mappings[i]),
                                        TypeCode.Decimal => reader.GetDecimal(mappings[i]),
                                        TypeCode.DateTime => reader.GetDateTime(mappings[i]),
                                        TypeCode.String => reader.GetString(mappings[i]),
                                        TypeCode.Object => reader.GetValue(mappings[i]),
                                        _ => throw new Exception($"Cannot parse value of type {conversionType}."),
                                    };
                                    property.SetValue(record, value);
                                }
                            } catch (Exception) {
                                throw new Exception($"failed to map value for field {property.Name}.");
                            }
                        }
                    }
                } catch (Exception ex) {
                    throw new Exception($"Error reading record {recordCounter}: {ex.Message}");
                }
                recordCounter++;
                yield return record;
            }
        }

        /// <summary>
        /// Returns the index of the first header matching the property.
        /// If no header matches the property, then -1 is returned.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static int findMatchingHeader(
            PropertyInfo property,
            List<string> headers
        ) {
            if (property.GetAttribute<IgnoreFieldAttribute>(false) != null) {
                return -1;
            }
            var required = property.GetAttribute<RequiredFieldAttribute>(false)?.Required ?? true;

            var columnIndexAttribute = property.GetAttribute<ColumnIndexAttribute>(false);
            if (columnIndexAttribute != null) {
                var index = columnIndexAttribute.ColumnIndex;
                if (index >= headers.Count) {
                    index = -1;
                } else if (index >= 0) {
                    return index;
                }
            }

            var acceptedNameAttributes = property.GetAttributes<AcceptedNameAttribute>(false);
            if (acceptedNameAttributes.Any()) {
                foreach (var acceptedNameAtrribute in acceptedNameAttributes) {
                    var index = headers
                        .FirstIndexMatch(h => acceptedNameAtrribute.Validate(h));
                    if (index >= 0) {
                        return index;
                    }
                }
            }

            if (required) {
                throw new Exception($"No mapping found for property {property.Name}.");
            } else {
                return -1;
            }
        }
    }
}
