using MCRA.Utils.DataSourceReading.Attributes;
using MCRA.Utils.ExtensionMethods;
using System.Data;

namespace MCRA.Utils.DataFileReading {

    public static class TableDefinitionExtensions {

        /// <summary>
        /// Returns an integer array of the same length as the column definition
        /// collection specifying the index of the matching header of the headers
        /// list for each column definition. If no matching header is found for a
        /// column definition, then the index is set to -1.
        /// </summary>
        /// <param name="columnDefinitions"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static int[] GetColumnMappings(
            this ICollection<ColumnDefinition> columnDefinitions,
            List<string> headers
        ) {
            var result = new int[columnDefinitions.Count];
            for (int i = 0; i < columnDefinitions.Count; i++) {
                var column = columnDefinitions.ElementAt(i);
                result[i] = column.GetMatchingHeaderIndex(headers);
            }
            return result;
        }

        /// <summary>
        /// Returns a new DataTable instance with the structure of the column definitions.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public static DataTable CreateDataTable(this TableDefinition tableDefinition) {
            var table = new DataTable(tableDefinition.TargetDataTable);
            foreach (var cd in tableDefinition.ColumnDefinitions.Where(cd => !cd.IsDynamic)) {
                Type columnType;
                switch (cd.GetFieldType()) {
                    case FieldType.Undefined:
                        columnType = typeof(string);
                        break;
                    case FieldType.AlphaNumeric:
                        columnType = typeof(string);
                        break;
                    case FieldType.Numeric:
                        columnType = typeof(double);
                        break;
                    case FieldType.Boolean:
                        columnType = typeof(bool);
                        break;
                    case FieldType.Integer:
                        columnType = typeof(int);
                        break;
                    case FieldType.DateTime:
                        columnType = typeof(DateTime);
                        break;
                    case FieldType.FileReference:
                        columnType = typeof(string);
                        break;
                    default:
                        throw new NotImplementedException("Unknown field type");
                }
                var column = new DataColumn(cd.Id, columnType);
                table.Columns.Add(column);
            }
            return table;
        }

        /// <summary>
        /// Creates a table definition from an object type based on the
        /// fields, field-types, and annotations.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TableDefinition FromType(Type type) {
            var columnDefinitions = new List<ColumnDefinition>();
            var tableDefinition = new TableDefinition() {
                Id = type.Name,
                ColumnDefinitions = columnDefinitions
            };
            tableDefinition.Aliases = type
                .GetAttributes<AcceptedNameAttribute>(false)
                .Select(r => r.AcceptedName)
                .ToHashSet();

            var properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++) {
                var property = properties[i];
                if (property.GetAttribute<IgnoreFieldAttribute>(false) != null) {
                    continue;
                }
                var columnDefinition = new ColumnDefinition();
                columnDefinition.Id = property.Name;
                columnDefinition.Aliases = property
                    .GetAttributes<AcceptedNameAttribute>(false)
                    .Select(r => r.AcceptedName)
                    .ToHashSet();

                if (property.PropertyType.IsEnum) {
                    // Enum value type
                    columnDefinition.FieldType = property.PropertyType.Name;
                } else {
                    columnDefinition.FieldType = FieldTypeConverter.FromSystemType(property.PropertyType).ToString();
                }

                if (property.GetAttribute<RequiredFieldAttribute>(false) != null) {
                    // Required attribute detected
                    columnDefinition.Required = true;
                } else if (property.PropertyType.IsClass || Nullable.GetUnderlyingType(property.PropertyType) != null) {
                    // Class or nullable value type can be null
                    columnDefinition.Required = false;
                } else {
                    // Non-nullable value type
                    columnDefinition.Required = true;
                }

                columnDefinitions.Add(columnDefinition);
            }
            return tableDefinition;
        }
    }
}
