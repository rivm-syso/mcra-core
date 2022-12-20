using MCRA.Utils.DataSourceReading.ValueConversion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// This is a csv data reader that implements the IDataReader interface.
    /// </summary>
    public class CsvDataReader : IDataReader, IDisposable {

        private readonly ValueConverterCollection _valueConverters;

        private readonly char _delimiter;
        private readonly char _comment = '#';

        private readonly bool _firstRowHeader = true;
        private readonly bool _trimUnquotedFields = true;

        private readonly int _csvColumnCount = 0;
        private readonly string[] _headers;
        private readonly List<Type> _fieldTypes = new();

        private StreamReader _stream;
        private string _csvlinestring = "";

        private int _rowCount;
        private object[] _line;

        /// <summary>
        /// Creates an instance of Csv reader
        /// </summary>
        /// <param name="stream">Text stream containing the csv data.</param>
        /// <param name="delimiter">delimiter character used in csv file.</param>
        /// <param name="firstRowHeader">specify the csv got a header in first row or not.
        /// Default is true and if argument is false then auto header 'ROW_xx will be used as per
        /// the order of columns.</param>
        /// <param name="fieldTypes">Specify an array of system column types as string.</param>
        /// <param name="encoding"></param>
        /// <param name="allowDuplicateHeaders">Don't throw exception when duplicate headers are added</param>
        /// <param name="valueConverters">Value converters to be used for parsing the csv field string values to system types.</param>
        public CsvDataReader(
            Stream stream,
            char delimiter = ',',
            bool firstRowHeader = true,
            Type[] fieldTypes = null,
            Encoding encoding = null,
            bool allowDuplicateHeaders = false,
            ValueConverterCollection valueConverters = null
        ) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            _valueConverters = valueConverters ?? ValueConverterCollection.Default();
            _stream = new StreamReader(stream, encoding);
            _delimiter = delimiter;
            _firstRowHeader = firstRowHeader;

            if (_firstRowHeader == true) {
                Read();

                _headers = ParseRow(_csvlinestring, _delimiter, _comment);
                _csvColumnCount = _headers.Length;
                if (!allowDuplicateHeaders) {
                    checkDuplicateHeaders();
                }
            } else {
                // Just open and close the file with read of first line to determine how many
                // rows are there and then add default rows as  row1,row2 etc to collection.
                Read();

                _headers = ParseRow(_csvlinestring, _delimiter, _comment);
                _headers = _headers.Select((r, ix) => $"COL_{ix}").ToArray();
                _csvColumnCount = _headers.Length;

                //set position in stream to 0
                stream.Seek(0, SeekOrigin.Begin);
                _stream = new StreamReader(stream, encoding);
            }

            // Fill column header types
            for (int i = 0; i < _headers.Length; i++) {
                Type columnType = null;
                if (fieldTypes != null && i < fieldTypes.Length) {
                    columnType = fieldTypes[i];
                }
                _fieldTypes.Add(columnType);
            }

            _csvlinestring = "";
            _line = null;
            _rowCount = 0;
        }

        /// <summary>
        /// Returns an array of header names as string in the order of columns 
        /// from left to right of csv file. If Csv file doesn't have header then a dummy header 
        /// with 'COL_' + 'column position' will be returned. This can be manually renamed calling 
        /// 'RenameCsvHeader'
        /// </summary>
        public string[] Header {
            get { return _headers; }
        }

        /// <summary>
        /// Returns an array of strings from the current line of csv file. 
        /// Call Read() method to read the next line/record of csv file. 
        /// </summary>
        public object[] Line {
            get {
                return _line;
            }
        }

        /// <summary>
        /// Gets the column with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name] {
            get { return Line[GetOrdinal(name)]; }
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
        /// Closes the stream
        /// </summary>
        public void Close() {
            if (_stream != null) {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }
        }

        /// <summary>
        /// Gets a value that indicates the depth of nesting for the current row.
        /// </summary>
        public int Depth {
            get { return 1; }
        }

        /// <summary>
        /// Gets whether the reader is closed.
        /// </summary>
        public bool IsClosed {
            get { return _stream == null; }
        }

        /// <summary>
        /// Returns how many records read so far.
        /// </summary>
        public int RecordsAffected {
            get { return _rowCount; }
        }

        /// <summary>
        /// Disposes the reader.
        /// </summary>
        public void Dispose() {
            if (_stream != null) {
                _stream.Dispose();
                _stream = null;
            }
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public int FieldCount {
            get { return Header.Length; }
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetBoolean(int i) {
            return GetValue<bool>(i);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i) {
            return GetValue<byte>(i);
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i) {
            return GetValue<char>(i);
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an IDataReader for the specified column ordinal.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i) {
            return this;
        }

        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i) {
            return GetValue<DateTime>(i);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i) {
            return GetValue<decimal>(i);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i) {
            return GetValue<double>(i);
        }

        /// <summary>
        /// Gets the Type information corresponding to the type of Object that would be returned from GetValue.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i) {
            return _fieldTypes[i] ?? typeof(string);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i) {
            return GetValue<float>(i);
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i) {
            return Guid.Parse(Line[i].ToString());
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i) {
            return GetValue<short>(i);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i) {
            return GetValue<int>(i);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i) {
            return GetValue<long>(i);
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i) {
            return Header[i];
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name) {
            int result = -1;
            for (int i = 0; i < _headers.Length; i++) {
                if (_headers[i] == name) {
                    return i;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the SqlDataReader.
        /// </summary>
        /// <returns>A DataTable that describes the column metadata.</returns>
        public DataTable GetSchemaTable() {
            var t = new DataTable();
            t.Columns.Add("ColumnName", typeof(string));
            t.Columns.Add("ColumnOrdinal", typeof(int));
            t.Columns.Add("ColumnSize", typeof(int));
            t.Columns.Add("DataType", typeof(Type));
            for (int i = 0; i < Header.Length; i++) {
                t.Rows.Add(Header[i], i, 2048, GetFieldType(i));
            }
            return t;
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i) {
            return Line[i]?.ToString();
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i) {
            return Line[i];
        }

        /// <summary>
        /// Gets the value of the specified field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i"></param>
        /// <returns></returns>
        public T GetValue<T>(int i) {
            var value = Line[i];
            if (_fieldTypes[i] != null) {
                // Value already parsed
                return (T)value;
            } else {
                // Value not yet parsed, parse now
                return (T)_valueConverters.Convert((string)value, typeof(T));
            }
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values) {
            var len = Line.Length;
            Array.Copy(Line, values, len);
            return len;
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i) {
            if (_fieldTypes[i] != null) {
                // Value already parsed
                return Line[i] == null;
            } else {
                // Value not yet parsed, check for null or empty
                return string.IsNullOrEmpty((string)Line[i]);
            }
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns></returns>
        public bool NextResult() {
            return Read();
        }

        /// <summary>
        /// Advances the IDataReader to the next record.
        /// Tries to read the next line of the csv.
        /// Returns true when it succeeded.
        /// </summary>
        /// <returns></returns>
        public bool Read() {
            if (!_stream.EndOfStream) {
                string[] line;
                do {
                    _csvlinestring = _stream.ReadLine();
                    line = ParseRow(_csvlinestring, _delimiter, _comment);
                } while (!_stream.EndOfStream && line.Length == 0);
                if (line.Length != 0) {
                    _line = readTypedRowValues(line);
                    _rowCount++;
                    return true;
                }
            }
            return false;
        }

        private object[] readTypedRowValues(string[] csvRowValues) {
            if (_csvColumnCount == 0 || csvRowValues.Length == 0) {
                return csvRowValues;
            }

            var length = _csvColumnCount;
            var typedValues = new object[length];

            // if header types array length doesn't match the destination array
            // return the values as-is
            if (_fieldTypes.Count != length) {
                Array.Copy(csvRowValues, 0, typedValues, 0, _csvColumnCount);
            } else {
                var offset = 0;
                foreach (var value in csvRowValues) {
                    if (_fieldTypes[offset] == null) {
                        typedValues[offset] = value;
                    } else if (!string.IsNullOrWhiteSpace(value)) {
                        typedValues[offset] = _valueConverters.Convert(value, _fieldTypes[offset]);
                    }
                    if (++offset >= length) {
                        break;
                    }
                }
            }

            return typedValues;
        }

        /// <summary>
        /// Reads a row of data from a Csv file
        /// </summary>
        /// <returns>array of strings from csv line</returns>
        private string[] readRow(string line) {
            var values = new List<string>();
            if (string.IsNullOrEmpty(line)) {
                return new string[] { };
            }

            int pos = 0;
            int rows = 0;
            while (pos < line.Length) {
                string value;

                // Skip leading whitespaces
                if (_trimUnquotedFields) {
                    while (pos < line.Length && char.IsWhiteSpace(line[pos])) {
                        pos++;
                    }
                    if (pos >= line.Length) {
                        continue;
                    }
                }

                // Special handling for quoted field
                if (line[pos] == '"') {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    int start = pos;
                    while (pos < line.Length) {
                        // Test for quote character
                        if (line[pos] == '"') {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= line.Length || line[pos] != '"') {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = line.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                } else {
                    // Parse unquoted value
                    int start = pos;
                    while (pos < line.Length && line[pos] != _delimiter) {
                        pos++;
                    }

                    value = line.Substring(start, pos - start);
                    if (_trimUnquotedFields) {
                        value = value.Trim();
                    }
                }
                // Add field to list
                if (rows < values.Count) {
                    values[rows] = value;
                } else {
                    values.Add(value);
                }

                rows++;

                // Eat up to and including next comma
                while (pos < line.Length && line[pos] != _delimiter) {
                    pos++;
                }

                if (pos < line.Length) {
                    pos++;
                }
            }
            // Empty columns at end of line string: fill with empty string values
            while (values.Count < _csvColumnCount) {
                values.Add(string.Empty);
            }
            return values.ToArray();
        }

        /// <summary>
        /// Reads a row of data from a Csv file
        /// </summary>
        /// <returns>array of strings from csv line</returns>
        /// <param name="line"></param>
        /// <param name="delimiter"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static string[] ParseRow(string line, char delimiter, char comment) {
            var values = new List<string>();

            if (string.IsNullOrEmpty(line) || line.StartsWith($"{comment}")) {
                return Array.Empty<string>();
            }

            var pos = 0;
            while (pos < line.Length) {
                string value;

                // Skip leading whitespaces
                if (char.IsWhiteSpace(line[pos]) && pos != line.Length -1) {
                    pos++;
                    continue;
                }

                // Special handling for quoted field
                if (line[pos] == '"') {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    var start = pos;
                    while (pos < line.Length) {
                        // Test for quote character
                        if (line[pos] == '"') {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= line.Length || line[pos] != '"') {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = line.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                } else {
                    // Parse unquoted value
                    var start = pos;
                    while (pos < line.Length && line[pos] != delimiter) {
                        pos++;
                    }
                    value = line.Substring(start, pos - start);

                    // Trim all leading and trailing whiltespaces
                    value = value.Trim();
                }

                // Add field to list
                values.Add(value);

                // Eat up to and including next comma
                while (pos < line.Length && line[pos] != delimiter) {
                    pos++;
                }

                // If the last character is a delimiter, then add empty string and break
                if (pos == line.Length -1 && line[pos] == delimiter) {
                    values.Add(string.Empty);
                    break;
                }

                if (pos < line.Length) {
                    pos++;
                }
            }

            return values.ToArray();
        }

        /// <summary>
        /// Check for duplicate headers. Throw exception when this is so.
        /// </summary>
        private void checkDuplicateHeaders() {
            // Check for duplicate headers
            var headercollection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _headers.Length; i++) {
                if (headercollection.Contains(_headers[i])) {
                    throw new Exception("Duplicate found in CSV header. Cannot create a CSV reader instance with duplicate header");
                }
                headercollection.Add(_headers[i]);
            }
        }
    }
}
