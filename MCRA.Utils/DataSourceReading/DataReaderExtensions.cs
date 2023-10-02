using System.ComponentModel;
using System.Data;
using System.Globalization;

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
        public static List<T> ReadRecords<T>(
            this IDataReader reader,
            TableDefinition tableDefinition
        ) where T : new() {
            var records = new List<T>();
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
                while (reader.Read()) {
                    var record = new T();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (columnMappings.TryGetValue(i, out var targetPropertyName)) {
                            if (targetRecordType.GetProperty(targetPropertyName) != null) {
                                var targetPropertyType = targetRecordType.GetProperty(targetPropertyName).PropertyType;
                                var value = convertToType(reader, i, targetPropertyType);
                                targetRecordType.GetProperty(targetPropertyName).SetValue(record, value, null);
                            }
                        }
                    }
                    records.Add(record);
                }
            }
            return records;
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
    }
}
