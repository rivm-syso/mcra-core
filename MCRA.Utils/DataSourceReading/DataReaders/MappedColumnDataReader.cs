using System.Data;

namespace MCRA.Utils.DataSourceReading.DataSourceReaders {

    /// <summary>
    /// Encapsulates an IDataReader and adds a virtual column with a constant value
    /// in the front of the data table.
    /// </summary>
    public sealed class MappedColumnDataReader : IDataReader {

        private readonly IDataReader _internalReader;
        private readonly int[] _columnMappings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnMappings"></param>
        public MappedColumnDataReader(IDataReader reader, int[] columnMappings) {
            _internalReader = reader;
            _columnMappings = columnMappings;
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
        /// Gets the column with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name] {
            get { return GetValue(GetOrdinal(name)); }
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        public int Depth {
            get { return _internalReader.Depth; }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        public bool IsClosed {
            get { return _internalReader.IsClosed; }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        public int RecordsAffected {
            get { return _internalReader.RecordsAffected; }
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public int FieldCount {
            get { return _columnMappings.Length; }
        }

        /// <summary>
        /// Closes the IDataReader Object.
        /// </summary>
        public void Close() {
            _internalReader.Close();
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetBoolean(int i) {
            return _internalReader.GetBoolean(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i) {
            return _internalReader.GetByte(_columnMappings[i]);
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            return _internalReader.GetBytes(_columnMappings[i], fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i) {
            return _internalReader.GetChar(_columnMappings[i]);
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldoffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            return _internalReader.GetChars(_columnMappings[i], fieldoffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Returns an IDataReader for the specified column ordinal.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i) {
            return _internalReader.GetData(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i) {
            return _internalReader.GetDataTypeName(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i) {
            return _internalReader.GetDateTime(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i) {
            return _internalReader.GetDecimal(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i) {
            return _internalReader.GetDouble(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the Type information corresponding to the type of Object that would be returned from GetValue.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i) {
            return _internalReader.GetFieldType(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i) {
            return _internalReader.GetFloat(_columnMappings[i]);
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i) {
            return _internalReader.GetGuid(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i) {
            return _internalReader.GetInt16(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i) {
            return _internalReader.GetInt32(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i) {
            return _internalReader.GetInt64(_columnMappings[i]);
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i) {
            return _internalReader.GetName(_columnMappings[i]);
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name) {
            var internalIndex = _internalReader.GetOrdinal(name);
            return Array.IndexOf(_columnMappings, internalIndex);
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the IDataReader.
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable() {
            DataTable schemaTable = new DataTable();
            schemaTable.Columns.Add("ColumnName", typeof(string));
            schemaTable.Columns.Add("ColumnOrdinal", typeof(int));
            schemaTable.Columns.Add("ColumnSize", typeof(int));
            schemaTable.Columns.Add("DataType", typeof(Type));
            var internalTableSchema = _internalReader.GetSchemaTable();
            for (int i = 0; i < _columnMappings.Length; i++) {
                var row = internalTableSchema.Rows[i];
                schemaTable.Rows.Add(row["ColumnName"], i, row["ColumnSize"], row["DataType"]);
            }
            return schemaTable;
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i) {
            return _internalReader.GetString(_columnMappings[i]);
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i) {
            return _internalReader.GetValue(_columnMappings[i]);
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values) {
            var internalValues = new object[FieldCount];
            _internalReader.GetValues(internalValues);
            for (int i = 1; i < FieldCount; i++) {
                values[i] = internalValues[i - 1];
            }
            return FieldCount;
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i) {
            if (_columnMappings[i] < 0 || _columnMappings[i] >= _internalReader.FieldCount) {
                return true;
            }
            return _internalReader.IsDBNull(_columnMappings[i]);
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
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
            return _internalReader.Read();
        }

        #region IDisposable Support
        public void Dispose() {
        }
        #endregion
    }
}
