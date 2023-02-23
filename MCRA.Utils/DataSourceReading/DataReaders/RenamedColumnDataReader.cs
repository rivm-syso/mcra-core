using System.Data;

namespace MCRA.Utils.DataSourceReading.DataSourceReaders {

    /// <summary>
    /// Encapsulates an IDataReader, renaming the specified column with
    /// a new column name.
    /// </summary>
    public sealed class RenamedColumnDataReader : IDataReader {

        private readonly IDataReader _internalReader;
        private readonly string _oldColumnName;
        private readonly string _newColumnName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="newColumnName"></param>
        public RenamedColumnDataReader(IDataReader reader, string oldColumnName, string newColumnName) {
            _internalReader = reader;
            _oldColumnName = oldColumnName;
            _newColumnName = newColumnName;
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
            get { return _internalReader.FieldCount; }
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
            return _internalReader.GetBoolean(i);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i) {
            return _internalReader.GetByte(i);
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
            return _internalReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i) {
            return _internalReader.GetChar(i);
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
            return _internalReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Returns an IDataReader for the specified column ordinal.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i) {
            return _internalReader.GetData(i);
        }

        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i) {
            return _internalReader.GetDataTypeName(i);
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i) {
            return _internalReader.GetDateTime(i);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i) {
            return _internalReader.GetDecimal(i);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i) {
            return _internalReader.GetDouble(i);
        }

        /// <summary>
        /// Gets the Type information corresponding to the type of Object that would be returned from GetValue.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i) {
            return _internalReader.GetFieldType(i);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i) {
            return _internalReader.GetFloat(i);
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i) {
            return _internalReader.GetGuid(i);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i) {
            return _internalReader.GetInt16(i);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i) {
            return _internalReader.GetInt32(i);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i) {
            return _internalReader.GetInt64(i);
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i) {
            var name = _internalReader.GetName(i);
            if (name.Equals(_oldColumnName, StringComparison.InvariantCultureIgnoreCase)) {
                return _newColumnName;
            }
            return name;
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name) {
            if (name.Equals(_newColumnName, StringComparison.InvariantCultureIgnoreCase)) {
                return _internalReader.GetOrdinal(_oldColumnName);
            }
            return _internalReader.GetOrdinal(name);
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
            var counter = 1;
            foreach (DataRow row in internalTableSchema.Rows) {
                string name = row["ColumnName"].ToString();
                if (!string.Equals(name, _oldColumnName, StringComparison.OrdinalIgnoreCase)) {
                    name = _newColumnName;
                }
                schemaTable.Rows.Add(name, counter, row["ColumnSize"], row["DataType"]);
                counter++;
            }
            return schemaTable;
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i) {
            return _internalReader.GetString(i);
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i) {
            return _internalReader.GetValue(i);
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values) {
            return _internalReader.GetValues(values);
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i) {
            return _internalReader.IsDBNull(i);
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
