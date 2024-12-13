using System.Data;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// CheckedOleDbTableReader: Wraps an IDataReader and performs checks on contents
    /// based on table definition's column definitions for the specific table and
    /// throws a more descriptive exception.
    /// </summary>
    public class TableDefinitionDataReader : IDataReader, IDisposable {

        private readonly IDataReader _internalReader;
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
        ) {
            _internalReader = internalReader;
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
        /// Gets the column located at the specified index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object this[int i] {
            get { return GetValue(i); }
        }

        /// <summary>
        /// Get the value of the column with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name] {
            get {
                var idx = _internalReader.GetOrdinal(_internalColumnNameMappings[name]);
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
        public int Depth => _internalReader.Depth;

        /// <summary>
        ///
        /// </summary>
        public bool IsClosed => _internalReader.IsClosed;

        /// <summary>
        ///
        /// </summary>
        public int RecordsAffected => _internalReader.RecordsAffected;

        /// <summary>
        ///
        /// </summary>
        public int FieldCount => _internalReader.FieldCount;

        /// <summary>
        ///
        /// </summary>
        public void Close() {
            _internalReader.Close();
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose() {
            _internalReader.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetBoolean(int i) {
            return _internalReader.GetBoolean(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i) {
            return _internalReader.GetByte(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            return _internalReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i) {
            return _internalReader.GetChar(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length) {
            return _internalReader.GetChars(i, fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i) {
            return _internalReader.GetData(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i) {
            return _internalReader.GetDataTypeName(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i) {
            return _internalReader.GetDateTime(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i) {
            return _internalReader.GetDecimal(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i) {
            return _internalReader.GetDouble(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i) {
            if (_columnDefinitions[i] != null) {
                var type = FieldTypeConverter.ToSystemType(_columnDefinitions[i].GetFieldType());
                if (!_columnDefinitions[i].Required && !type.IsClass && Nullable.GetUnderlyingType(type) == null) {
                    return typeof(Nullable<>).MakeGenericType(type);
                }
                return type;
            }
            return _internalReader.GetFieldType(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i) {
            return _internalReader.GetFloat(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i) {
            return _internalReader.GetGuid(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i) {
            return _internalReader.GetInt16(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i) {
            return _internalReader.GetInt32(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i) {
            return _internalReader.GetInt64(i);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i) {
            return _columnNames[i];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name) {
            if (_columnIndexes.TryGetValue(name, out var index)) {
                return index;
            }
            return -1;
        }

        /// <summary>
        /// Build schema table based on column definitions
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable() {
            DataTable schemaTable = new DataTable();
            schemaTable.Columns.Add("ColumnName", typeof(string));
            schemaTable.Columns.Add("ColumnOrdinal", typeof(int));
            schemaTable.Columns.Add("ColumnSize", typeof(int));
            schemaTable.Columns.Add("DataType", typeof(Type));
            schemaTable.Columns.Add("AllowDbNull", typeof(bool));
            schemaTable.Columns.Add("IsUnique", typeof(bool));
            for (int i = 0; i < _internalReader.FieldCount; i++) {
                schemaTable.Rows.Add(
                    _columnNames[i],
                    i,
                    _columnDefinitions[i]?.FieldSize ?? -1,
                    _columnDefinitions[i] != null
                        ? FieldTypeConverter.ToSystemType(_columnDefinitions[i].GetFieldType())
                        : _internalReader.GetFieldType(i),
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
        public string GetString(int i) {
            return getCheckedStringValue(i);
        }

        /// <summary>
        /// Gets the value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i) {
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
                return _internalReader.GetValue(i);
            }
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values) {
            for (int i = 0; i < values.Length; i++) {
                values[i] = GetValue(i);
            }
            return values.Length;
        }

        /// <summary>
        /// Checks if the value at the specified index is DBNull.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i) {
            return _internalReader.IsDBNull(i);
        }

        /// <summary>
        /// Advances the data reader to the next result.
        /// </summary>
        /// <returns></returns>
        public bool NextResult() {
            return _internalReader.NextResult();
        }

        /// <summary>
        /// Advances the IDataReader to the next record.
        /// </summary>
        /// <returns></returns>
        public bool Read() {
            RowCount++;
            var result = _internalReader.Read();
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
            var value = _internalReader.GetValue(i).ToString();
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
