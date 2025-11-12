using System.Data;

namespace MCRA.Utils.DataSourceReading.DataReaders {
    public abstract class DataReaderBase(IDataReader reader) : IDataReader {

        private readonly IDataReader _internalReader = reader;

        #region IDataReader methods delegated to internal reader
        // All IDataReader implemented methods that retrieve a column (value) by index use the
        // virtual callback to get the actual index for the internal reader
        // This allows a subclass to implement an index mapping without having to override
        // all Get[..] methods to provide an index mapping, only the callback function

        public virtual object this[int i] => _internalReader.GetValue(TranslateFieldIndex(i));
        public virtual object this[string name] => _internalReader.GetValue(_internalReader.GetOrdinal(name));
        public virtual int Depth => _internalReader.Depth;
        public virtual bool IsClosed => _internalReader.IsClosed;
        public virtual int RecordsAffected => _internalReader.RecordsAffected;
        public virtual int FieldCount => _internalReader.FieldCount;
        public virtual void Close() => _internalReader.Close();
        public virtual bool GetBoolean(int i) => _internalReader.GetBoolean(TranslateFieldIndex(i));
        public virtual byte GetByte(int i) => _internalReader.GetByte(TranslateFieldIndex(i));
        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
            _internalReader.GetBytes(TranslateFieldIndex(i), fieldOffset, buffer, bufferoffset, length);
        public virtual char GetChar(int i) => _internalReader.GetChar(TranslateFieldIndex(i));
        public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) =>
            _internalReader.GetChars(TranslateFieldIndex(i), fieldoffset, buffer, bufferoffset, length);
        public virtual IDataReader GetData(int i) => _internalReader.GetData(TranslateFieldIndex(i));
        public virtual string GetDataTypeName(int i) => _internalReader.GetDataTypeName(TranslateFieldIndex(i));
        public virtual DateTime GetDateTime(int i) => _internalReader.GetDateTime(TranslateFieldIndex(i));
        public virtual decimal GetDecimal(int i) => _internalReader.GetDecimal(TranslateFieldIndex(i));
        public virtual double GetDouble(int i) => _internalReader.GetDouble(TranslateFieldIndex(i));
        public virtual Type GetFieldType(int i) => _internalReader.GetFieldType(TranslateFieldIndex(i));
        public virtual float GetFloat(int i) => _internalReader.GetFloat(TranslateFieldIndex(i));
        public virtual Guid GetGuid(int i) => _internalReader.GetGuid(TranslateFieldIndex(i));
        public virtual short GetInt16(int i) => _internalReader.GetInt16(TranslateFieldIndex(i));
        public virtual int GetInt32(int i) => _internalReader.GetInt32(TranslateFieldIndex(i));
        public virtual long GetInt64(int i) => _internalReader.GetInt64(TranslateFieldIndex(i));
        public virtual string GetName(int i) => _internalReader.GetName(TranslateFieldIndex(i));
        public virtual int GetOrdinal(string name) => _internalReader.GetOrdinal(name);
        public virtual DataTable GetSchemaTable() => _internalReader.GetSchemaTable();
        public virtual string GetString(int i) => _internalReader.GetString(TranslateFieldIndex(i));
        public virtual object GetValue(int i) => _internalReader.GetValue(TranslateFieldIndex(i));
        public virtual int GetValues(object[] values) => _internalReader.GetValues(values);
        public virtual bool IsDBNull(int i) => _internalReader.IsDBNull(TranslateFieldIndex(i));
        public virtual bool NextResult() => _internalReader.NextResult();
        public virtual bool Read() => _internalReader.Read();
        #endregion

        /// <summary>
        /// Overridable function to map an index of the implementing class
        /// to the internal reader's index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected virtual int TranslateFieldIndex(int i) => i;

        #region Disposable
        private bool _disposed;

        ~DataReaderBase() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _internalReader?.Close();
                    _internalReader?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
