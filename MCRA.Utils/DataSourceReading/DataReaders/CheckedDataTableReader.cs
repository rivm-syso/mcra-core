using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace MCRA.Utils.DataFileReading {
    /// <summary>
    /// CheckedOleDbTableReader: Wraps the Access and Excel table datareader and performs
    /// checks on contents based on table definition's column definitions for the specific table
    /// and throws a more descriptive exception
    /// </summary>
    public class CheckedDataTableReader : IDataReader, IDisposable {

        private readonly IDataReader _internalReader;
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
        ) {
            _internalReader = internalReader;
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
                _colNames = values.Select(r => Convert.ToString(r)).ToArray();
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
        public object this[int i] {
            get {
                if (_fieldTypes[i] == FieldType.AlphaNumeric) {
                    return getCheckedStringValue(i, _internalReader.GetValue(i)?.ToString());
                }

                var coldef = _columnDefs[i];
                var val = _internalReader.GetValue(i);
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
        public object this[string name] {
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
            if (!_keepReaderOpen) {
                _internalReader.Close();
            }
        }

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
            return _colNames[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name) {
            return _fieldIdByNames[name];
        }

        /// <summary>
        /// Build schema table based on column definitions
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable() {
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
            return _internalReader.GetSchemaTable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i) {
            return getCheckedStringValue(i, _internalReader.GetString(i));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i) {
            var val = _internalReader.GetValue(i);

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
        public int GetValues(object[] values) {
            for (int i = 0; i < values.Length; i++) {
                values[i] = _internalReader.GetValue(i);
            }
            return values.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i) {
            return _internalReader.IsDBNull(i);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool NextResult() {
            return _internalReader.NextResult();
        }

        /// <summary>
        /// 
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
