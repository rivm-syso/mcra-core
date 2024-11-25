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
                var columnType = cd.GetFieldType() switch {
                    FieldType.Undefined => typeof(string),
                    FieldType.AlphaNumeric => typeof(string),
                    FieldType.Numeric => typeof(double),
                    FieldType.Boolean => typeof(bool),
                    FieldType.Integer => typeof(int),
                    FieldType.DateTime => typeof(DateTime),
                    FieldType.FileReference => typeof(string),
                    _ => throw new NotImplementedException("Unknown field type"),
                };
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
