using System.Data;
using System.Globalization;
using MCRA.Utils.DataSourceReading.DataReaders;

namespace MCRA.Utils.DataFileReading {
    /// <summary>
    /// CheckedOleDbTableReader: Wraps the Access and Excel table datareader and performs
    /// checks on contents based on table definition's column definitions for the specific table
    /// and throws a more descriptive exception
    /// </summary>
    public class CheckedDataTableReader : DataReaderBase {

        private readonly bool _hasFieldNamesHeader = false;
        private readonly bool _keepReaderOpen = false;
        private readonly ColumnDefinition[] _columnDefs;
        private readonly FieldType[] _fieldTypes;
        private readonly int[] _fieldSizes;
        private readonly Dictionary<string, int> _fieldIdByNames;
        private readonly string[] _colNames;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="internalReader"></param>
        /// <param name="tableDefinition"></param>
        /// <param name="hasFieldNamesHeader"></param>
        /// <param name="keepReaderOpen">Don't close underlying DataReader on Close</param>
        public CheckedDataTableReader(
            IDataReader internalReader,
            TableDefinition tableDefinition,
            bool hasFieldNamesHeader = false,
            bool keepReaderOpen = false
        ) : base(internalReader) {
            _keepReaderOpen = keepReaderOpen;
            _hasFieldNamesHeader = hasFieldNamesHeader;

            var colCount = internalReader.FieldCount;

            _columnDefs = new ColumnDefinition[colCount];
            _colNames = new string[colCount];
            _fieldTypes = new FieldType[colCount];
            _fieldSizes = new int[colCount];
            _fieldIdByNames = new Dictionary<string, int>(colCount);

            //increase current row count by 1 if table has field names (in Excel files)
            if (hasFieldNamesHeader) {
                Read();
                var values = new object[colCount];
                GetValues(values);
                _colNames = [.. values.Select(Convert.ToString)];
            } else {
                var schema = internalReader.GetSchemaTable();
                for (int i = 0; i < colCount; i++) {
                    var colName = schema.Rows[i].ItemArray[0].ToString();
                    _colNames[i] = colName;
                }
            }
            for (int i = 0; i < colCount; i++) {
                //column definition of first column(s) may be inserted (for example idDataSource)
                //these are not specified in the table definition, add a dummy column in this case
                var colDef = tableDefinition.FindColumnDefinitionByAlias(_colNames[i])
                          ?? new ColumnDefinition { Name = _colNames[i] };

                _columnDefs[i] = colDef;
                _fieldIdByNames[_colNames[i]] = i;
                _fieldTypes[i] = colDef.GetFieldType();
                _fieldSizes[i] = colDef.FieldSize;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override object this[int i] {
            get {
                if (_fieldTypes[i] == FieldType.AlphaNumeric) {
                    return getCheckedStringValue(i, base.GetValue(i)?.ToString());
                }

                var coldef = _columnDefs[i];
                var val = base.GetValue(i);
                if (val == null && coldef.Required) {
                    throw new ArgumentException($"Error on line {RowCount}: " +
                        $"Value in column '{_colNames[i]}' is required.");
                }
                return val;
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object this[string name] {
            get {
                var idx = _fieldIdByNames[name];
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
        public override void Close() {
            if (!_keepReaderOpen) {
                base.Close();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetName(int i) => _colNames[i];

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override int GetOrdinal(string name) => _fieldIdByNames[name];

        /// <summary>
        /// Build schema table based on column definitions
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTable() {
            //if field names header is specified (excel reader)
            //create a schema table, internalreader's schema table is
            //not implemented
            if (_hasFieldNamesHeader) {
                DataTable schemaTable = new DataTable();
                schemaTable.Columns.Add("ColumnName", typeof(string));
                schemaTable.Columns.Add("ColumnOrdinal", typeof(int));
                schemaTable.Columns.Add("ColumnSize", typeof(int));
                schemaTable.Columns.Add("DataType", typeof(Type));
                schemaTable.Columns.Add("AllowDbNull", typeof(bool));
                schemaTable.Columns.Add("IsUnique", typeof(bool));
                for (int i = 0; i < _columnDefs.Length; i++) {
                    schemaTable.Rows.Add(
                        _columnDefs[i].Id ?? _columnDefs[i].Name,
                        i,
                        _fieldSizes[i],
                        FieldTypeConverter.ToSystemType(_columnDefs[i].GetFieldType()),
                        !_columnDefs[i].Required,
                        _columnDefs[i].IsUnique
                    );
                }
                return schemaTable;
            }
            return base.GetSchemaTable();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetString(int i) => getCheckedStringValue(i, base.GetString(i));

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override object GetValue(int i) {
            var val = base.GetValue(i);

            if (_fieldTypes[i] == FieldType.AlphaNumeric) {
                //return checked (and trimmed) value
                return getCheckedStringValue(i, val?.ToString());
            }

            var coldef = _columnDefs[i];
            if (val == null && coldef.Required) {
                throw new ArgumentException($"Error on line {RowCount}: " +
                    $"Value in column '{_colNames[i]}' is required.");
            }
            //check whether returned value's type is correct
            //check for any numeric or boolean values passed in as strings
            if ((_fieldTypes[i] == FieldType.Numeric || _fieldTypes[i] == FieldType.Boolean)
                && val is string stringVal) {
                //fields in data source are erroneously typed as string, first check for null
                if (string.IsNullOrWhiteSpace(stringVal)) {
                    //for empty strings, return null,
                    //or throw exception if the column is required
                    if (coldef.Required) {
                        throw new ArgumentException($"Error on line {RowCount}: " +
                            $"Value in column '{_colNames[i]}' is required.");
                    }
                    return null;
                } else if (_fieldTypes[i] == FieldType.Numeric) {
                    var doubleVal = double.Parse(stringVal.Replace(',', '.'), NumberFormatInfo.InvariantInfo);
                    return doubleVal;
                    //all other cases: try parsing bool value
                } else if (bool.TryParse(stringVal, out var boolVal)) {
                    return boolVal;
                    //if it's an integer value (0 == false, otherwise true)
                } else if (int.TryParse(stringVal, out var intVal)) {
                    return intVal != 0;
                    //string comparison if all of the above fails
                    //we allow y/n, yes/no, t/f, true/false (case insensitive) here for now.
                } else if (stringVal.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                           stringVal.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                           stringVal.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                           stringVal.Equals("true", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                } else if (stringVal.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                       stringVal.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                       stringVal.Equals("f", StringComparison.OrdinalIgnoreCase) ||
                       stringVal.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
            }
            return val;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override int GetValues(object[] values) {
            for (int i = 0; i < values.Length; i++) {
                values[i] = base.GetValue(i);
            }
            return values.Length;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override bool Read() {
            RowCount++;
            var validRead = base.Read();
            if (validRead) {
                var values = new object[FieldCount];
                GetValues(values);
                //skip empty rows, check whether all values
                //are empty, keeping _skipCheckFieldCount into account
                while (validRead && values.All(v =>
                    v is DBNull ||
                    v is null ||
                    string.IsNullOrWhiteSpace(Convert.ToString(v)))
                ) {
                    //skip and read next record
                    RowCount++;
                    validRead = base.Read();
                    if (validRead) {
                        GetValues(values);
                    }
                }
            }
            return validRead;
        }

        private string getCheckedStringValue(int i, string value) {
            var coldef = _columnDefs[i];

            if (string.IsNullOrWhiteSpace(value)) {
                if (coldef.Required) {
                    throw new ArgumentException($"Error on line {RowCount}: " +
                        $"Value in column '{_colNames[i]}' is required.");
                }
                return string.Empty;
            }

            var stringVal = value.Trim();
            var size = _fieldSizes[i];
            if (size > 0 && stringVal.Length > size) {
                //current line is _recordsaffected + 1 (0-based) + 1 (column headers on 1st line)
                throw new ArgumentException($"Error on line {RowCount}: " +
                    $"Value in column '{_colNames[i]}' is {stringVal.Length} characters " +
                    $"long and exceeds the maximum size ({size})");
            }
            //return trimmed value
            return stringVal;
        }
    }
}
