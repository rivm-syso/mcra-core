using System.Data;
using System.IO.Compression;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source reader for reading zipped csv file collections.
    /// </summary>
    public class ZipByteArrayReader : DataSourceReaderBase {

        private readonly byte[] _zippedBuffer;

        private ZipArchive _zipArchive;

        private MemoryStream _memoryStream;

        private Dictionary<string, string> _zipEntries = new();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="zippedBuffer"></param>
        public ZipByteArrayReader(byte[] zippedBuffer) {
            _zippedBuffer = zippedBuffer;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~ZipByteArrayReader() {
            dispose(false);
        }

        /// <summary>
        /// Closes the reader connection.
        /// </summary>
        public override void Close() {
            if (_zipArchive != null) {
                _zipArchive.Dispose();
                _zipArchive = null;
                _memoryStream.Dispose();
                _memoryStream = null;
            }
        }

        /// <summary>
        /// Opens the reader connection.
        /// </summary>
        public override void Open() {
            if (_zipArchive == null && _memoryStream == null) {
                _memoryStream = new MemoryStream(_zippedBuffer);
                _zipArchive = new ZipArchive(_memoryStream);
                _zipEntries = _zipArchive.Entries
                    .Select(f => f.FullName)
                    .ToDictionary(s => Path.GetFileNameWithoutExtension(s));
            }
        }

        /// <summary>
        /// Implements <see cref="DataSourceReaderBase.GetTableNames()" />.
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTableNames() {
            try {
                return _zipEntries.Keys.ToList();
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Exception enumerating files in archive.\n" + ex.ToString());
                return new List<string>();
            }
        }

        /// <summary>
        /// Implements <see cref="DataSourceReaderBase.GetDataReaderByDefinition(TableDefinition, out string)" />.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableName"></param>
        /// <returns></returns>
        public override IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName) {
            try {
                var tableNames = GetTableNames();
                sourceTableName = tableNames.SingleOrDefault(s => tableDefinition.AcceptsName(s));
            } catch (Exception) {
                throw new Exception($"Multiple data tables found for {tableDefinition.Name}.");
            }
            if (sourceTableName != null) {
                var zipEntry = _zipArchive.GetEntry(_zipEntries[sourceTableName]);
                var textStream = zipEntry.Open();
                var dataReader = new TableDefinitionDataReader(new CsvDataReader(textStream), tableDefinition);
                return dataReader;
            }
            return null;
        }

        /// <summary>
        /// Override: returns a reader based on the provided table name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableDefinition"></param>
        /// 
        /// <returns></returns>
        public override IDataReader GetDataReaderByName(string tableName, TableDefinition tableDefinition) {
            var sourceTableName = GetTableNames().FirstOrDefault(s => string.Equals(tableName, s, StringComparison.InvariantCultureIgnoreCase));
            if (sourceTableName != null) {
                var zipEntry = _zipArchive.GetEntry(_zipEntries[sourceTableName]);
                var textStream = zipEntry.Open();
                if (tableDefinition != null) {
                    var dataReader = new TableDefinitionDataReader(new CsvDataReader(textStream), tableDefinition);
                    return dataReader;
                } else {
                    return new CsvDataReader(textStream);
                }
            }
            return null;
        }

        /// <summary>
        /// Evaluates columns against table definitions, Csv format has no
        /// rules for integer and double definition.
        /// </summary>
        /// <param name="columnDefinitions"></param>
        /// <param name="sourceTableReader"></param>
        public override void ValidateSourceTableColumns(ICollection<ColumnDefinition> columnDefinitions, IDataReader sourceTableReader) {
            var columnNames = new List<string>();
            var fieldTypes = new List<Type>(sourceTableReader.FieldCount);
            for (int i = 0; i < sourceTableReader.FieldCount; i++) {
                columnNames.Add(sourceTableReader.GetName(i).ToLower());
                fieldTypes.Add(sourceTableReader.GetFieldType(i));
            }
            var mappings = columnDefinitions.GetColumnMappings(columnNames);
            for (int i = 0; i < columnDefinitions.Count; i++) {
                if (mappings[i] > -1) {
                    // TODO: check field types
                }
            }
        }
    }
}
