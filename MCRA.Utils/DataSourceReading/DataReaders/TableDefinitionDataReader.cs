using System.Data;
using MCRA.Utils.DataSourceReading.DataReaders;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// CheckedOleDbTableReader: Wraps an IDataReader and performs checks on contents
    /// based on table definition's column definitions for the specific table and
    /// throws a more descriptive exception.
    /// </summary>
    public class TableDefinitionDataReader : DataReaderBase {

        private readonly List<ColumnDefinition> _columnDefinitions;
        private readonly List<string> _columnNames;
        private readonly Dictionary<string, int> _columnIndexes;
        private readonly Dictionary<string, string> _internalColumnNameMappings;
        private readonly bool _useDefinitionColumnNames;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="internalReader"></param>
        /// <param name="tableDefinition"></param>
        /// <param name="useDefinitionColumnNames"></param>
        public TableDefinitionDataReader(
            IDataReader internalReader,
            TableDefinition tableDefinition,
            bool useDefinitionColumnNames = false
        ) : base(internalReader) {
            _useDefinitionColumnNames = useDefinitionColumnNames;
            _columnNames = [];
            _columnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _internalColumnNameMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _columnDefinitions = [];
            var headers = internalReader.GetColumnNames();
            for (int i = 0; i < headers.Count; i++) {
                var columnDefinition = tableDefinition.FindColumnDefinitionByAlias(headers[i]);
                var name = _useDefinitionColumnNames
                    ? columnDefinition?.Id ?? headers[i]
                    : headers[i];
                _columnNames.Add(name);
                _columnIndexes[name] = i;
                _internalColumnNameMappings.Add(name, headers[i]);
                _columnDefinitions.Add(columnDefinition);
            }
        }

        /// <summary>
        /// Get the value of the column with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object this[string name] {
            get {
                var idx = base.GetOrdinal(_internalColumnNameMappings[name]);
                return this[idx];
            }
        }

        /// <summary>
        /// Current row of reader
        /// </summary>
        public int RowCount { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override Type GetFieldType(int i) {
            if (_columnDefinitions[i] != null) {
                var type = FieldTypeConverter.ToSystemType(_columnDefinitions[i].GetFieldType());
                if (!_columnDefinitions[i].Required && !type.IsClass && Nullable.GetUnderlyingType(type) == null) {
                    return typeof(Nullable<>).MakeGenericType(type);
                }
                return type;
            }
            return base.GetFieldType(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetName(int i) => _columnNames[i];

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override int GetOrdinal(string name) => _columnIndexes.TryGetValue(name, out var index) ? index : -1;

        /// <summary>
        /// Build schema table based on column definitions
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTable() {
            DataTable schemaTable = new DataTable();
            schemaTable.Columns.Add("ColumnName", typeof(string));
            schemaTable.Columns.Add("ColumnOrdinal", typeof(int));
            schemaTable.Columns.Add("ColumnSize", typeof(int));
            schemaTable.Columns.Add("DataType", typeof(Type));
            schemaTable.Columns.Add("AllowDbNull", typeof(bool));
            schemaTable.Columns.Add("IsUnique", typeof(bool));
            for (int i = 0; i < base.FieldCount; i++) {
                schemaTable.Rows.Add(
                    _columnNames[i],
                    i,
                    _columnDefinitions[i]?.FieldSize ?? -1,
                    _columnDefinitions[i] != null
                        ? FieldTypeConverter.ToSystemType(_columnDefinitions[i].GetFieldType())
                        : base.GetFieldType(i),
                    !_columnDefinitions[i]?.Required ?? true,
                    _columnDefinitions[i]?.IsUnique ?? false
                );
            }
            return schemaTable;
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetString(int i) => getCheckedStringValue(i);

        /// <summary>
        /// Gets the value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override object GetValue(int i) {
            var coldef = _columnDefinitions[i];
            if (coldef != null) {

                // Check if null
                if (IsDBNull(i)) {
                    // Check required
                    if (coldef.Required) {
                        throw new ArgumentException($"Error on line {RowCount}: " +
                            $"Value in column '{_columnNames[i]}' is required.");
                    } else {
                        return DBNull.Value;
                    }
                }

                var fieldType = coldef.GetFieldType();
                return fieldType switch {
                    FieldType.AlphaNumeric => GetString(i),
                    FieldType.Numeric => GetDouble(i),
                    FieldType.Boolean => GetBoolean(i),
                    FieldType.Integer => GetInt32(i),
                    FieldType.DateTime => GetDateTime(i),
                    FieldType.FileReference => GetString(i),
                    FieldType.Undefined => GetString(i),
                    _ => throw new Exception($"Unknown field type {fieldType}."),
                };
            } else {
                return base.GetValue(i);
            }
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override int GetValues(object[] values) {
            for (int i = 0; i < values.Length; i++) {
                values[i] = GetValue(i);
            }
            return values.Length;
        }

        /// <summary>
        /// Advances the IDataReader to the next record.
        /// </summary>
        /// <returns></returns>
        public override bool Read() {
            RowCount++;
            var result = base.Read();
            if (result) {
                var values = new object[FieldCount];
                GetValues(values);
                //skip empty rows, check whether all values
                //are empty, keeping _skipCheckFieldCount into account
                if (values.All(v =>
                    v is DBNull ||
                    v is null ||
                    string.IsNullOrWhiteSpace(Convert.ToString(v)))
                ) {
                    //skip and read next record
                    result = Read();
                }
            }
            return result;
        }

        private string getCheckedStringValue(int i) {
            var value = base.GetValue(i).ToString();
            if (_columnDefinitions[i] != null) {
                if (string.IsNullOrWhiteSpace(value)) {
                    if (_columnDefinitions[i].Required) {
                        throw new ArgumentException($"Error on line {RowCount}: " +
                            $"Value in column '{_columnNames[i]}' is required.");
                    }
                    return string.Empty;
                }

                var stringVal = value.Trim();
                if (_columnDefinitions[i].FieldSize > 0 && stringVal.Length > _columnDefinitions[i].FieldSize) {
                    throw new ArgumentException($"Error on line {RowCount}: " +
                        $"Value in column '{_columnNames[i]}' is {stringVal.Length} characters " +
                        $"long and exceeds the maximum size ({_columnDefinitions[i].FieldSize})");
                }
                return stringVal;
            } else {
                return value;
            }
        }
    }
}
