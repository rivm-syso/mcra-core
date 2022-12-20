using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MCRA.Data.Raw.Converters {

    /// <summary>
    /// Encapsulates an IDataReader and adds a virtual column with a constant value
    /// in the front of the data table.
    /// </summary>
    public sealed class RecodingDataReader : IDataReader {

        private readonly IDataReader _internalReader;
        private readonly IDictionary<string, string> _conversionTuples;

        private readonly string _recodedColumnName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="recodedColumnName"></param>
        /// <param name="virtualColumnValue"></param>
        public RecodingDataReader(IDataReader reader, string recodedColumnName, IDictionary<string, string> conversionTuples) {
            _internalReader = reader;
            _recodedColumnName = recodedColumnName;
            _conversionTuples = conversionTuples;
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
            get { return _internalReader.FieldCount + 1; }
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
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetBoolean(i - 1);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetByte(i - 1);
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
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetBytes(i - 1, fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetChar(i - 1);
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
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetChars(i - 1, fieldoffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Returns an IDataReader for the specified column ordinal.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetData(i - 1);
        }

        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i) {
            if (i == 0) {
                return typeof(string).Name;
            }
            return _internalReader.GetDataTypeName(i - 1);
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetDateTime(i - 1);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetDecimal(i - 1);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetDouble(i - 1);
        }

        /// <summary>
        /// Gets the Type information corresponding to the type of Object that would be returned from GetValue.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i) {
            if (i == 0) {
                return typeof(string);
            }
            return _internalReader.GetFieldType(i - 1);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetFloat(i - 1);
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetGuid(i - 1);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetInt16(i - 1);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetInt32(i - 1);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i) {
            if (i == 0) {
                throw new NotImplementedException();
            }
            return _internalReader.GetInt64(i - 1);
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i) {
            if (i == 0) {
                return _recodedColumnName;
            } else {
                var name = _internalReader.GetName(i - 1);
                if (name.Equals(_recodedColumnName, StringComparison.InvariantCultureIgnoreCase)) {
                    name = $"{_recodedColumnName}Orig"; 
                }
                return name;
            }
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name) {
            if (name.Equals(_recodedColumnName, StringComparison.InvariantCultureIgnoreCase)) {
                return 0;
            } else if (name.Equals($"{_recodedColumnName}Orig", StringComparison.InvariantCultureIgnoreCase)) {
                return _internalReader.GetOrdinal(_recodedColumnName) + 1;
            }
            return _internalReader.GetOrdinal(name) + 1;
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
            schemaTable.Rows.Add(_recodedColumnName, 0, 2048, typeof(string));
            var internalTableSchema = _internalReader.GetSchemaTable();
            var counter = 1;
            foreach (DataRow row in internalTableSchema.Rows) {
                schemaTable.Rows.Add(row["ColumnName"], counter, row["ColumnSize"], row["DataType"]);
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
            if (i == 0) {
                return getRecodedValue();
            }
            return _internalReader.GetString(i - 1);
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i) {
            if (i == 0) {
                return getRecodedValue();
            }
            return _internalReader.GetValue(i - 1);
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values) {
            values[0] = getRecodedValue();
            var internalValues = new object[FieldCount - 1];
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
            if (i == 0) {
                return string.IsNullOrEmpty(getRecodedValue());
            }
            return _internalReader.IsDBNull(i - 1);
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

        private string getRecodedValue() {
            var ix = _internalReader.GetOrdinal(_recodedColumnName);
            if (!_internalReader.IsDBNull(ix)) {
                var originalValue = _internalReader.GetString(ix);
                if (!string.IsNullOrEmpty(originalValue) && _conversionTuples.TryGetValue(originalValue, out var recodedValue)) {
                    return recodedValue;
                }
                return originalValue;
            }
            return null;
        }

        #region IDisposable Support
        public void Dispose() {
        }
        #endregion
    }
}
