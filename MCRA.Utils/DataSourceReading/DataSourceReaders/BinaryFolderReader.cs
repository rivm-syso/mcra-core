using System.Data;
using System.Diagnostics;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source reader for reading zipped csv file collections.
    /// </summary>
    public class BinaryFolderReader : DataFileReader, IDisposable {

        private Dictionary<string, string> _dataFiles = new();
        private Dictionary<string, string> _schemaFiles = new();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName"></param>
        public BinaryFolderReader(string fileName) : base(fileName) {
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~BinaryFolderReader() {
            dispose(false);
        }

        /// <summary>
        /// Closes the reader connection.
        /// </summary>
        public override void Close() {
        }

        /// <summary>
        /// Opens the reader connection.
        /// </summary>
        public override void Open() {
            _dataFiles = Directory.GetFiles(_filename, "*.bin")
                .ToDictionary(
                    s => Path.GetFileNameWithoutExtension(s),
                    s => Path.GetFileName(s), StringComparer.OrdinalIgnoreCase
                );
            _schemaFiles = Directory.GetFiles(_filename, "*.def")
                .ToDictionary(
                    s => Path.GetFileNameWithoutExtension(s),
                    s => Path.GetFileName(s), StringComparer.OrdinalIgnoreCase
                );
        }

        /// <summary>
        /// Implements <see cref="IDataSourceReader.GetTableNames()" />.
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTableNames() {
            try {
                return _dataFiles.Keys.ToList();
            } catch (Exception ex) {
                Debug.WriteLine("Exception enumerating files in archive.\n" + ex.ToString());
                return new List<string>();
            }
        }

        /// <summary>
        /// Implements <see cref="IDataSourceReader.GetDataReaderByDefinition(TableDefinition, out string)" />.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableName"></param>
        /// <returns></returns>
        public override IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName) {
            try {
                var tableNames = GetTableNames();
                sourceTableName = tableNames.SingleOrDefault(s => tableDefinition.AcceptsName(s));
            } catch (Exception) {
                throw new Exception($"Multiple data tables found for {tableDefinition.Name}");
            }
            if (sourceTableName != null) {
                return getDataReader(sourceTableName, tableDefinition);
            }
            return null;
        }

        /// <summary>
        /// Override: returns a reader based on the provided table name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public override IDataReader GetDataReaderByName(string tableName, TableDefinition tableDefinition = null) {
            var sourceTableName = GetTableNames().FirstOrDefault(s => string.Equals(tableName, s, StringComparison.InvariantCultureIgnoreCase));
            if (sourceTableName != null) {
                return getDataReader(sourceTableName, tableDefinition);
            }
            return null;
        }

        /// <summary>
        /// Evaluates columns against table definitions, Csv format has no
        /// rules for integer and double definition.
        /// </summary>
        /// <param name="columnDefinitions"></param>
        /// <param name="sourceTableReader"></param>
        public override void ValidateSourceTableColumns(
            ICollection<ColumnDefinition> columnDefinitions,
            IDataReader sourceTableReader
        ) {
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

        private IDataReader getDataReader(string sourceTableName, TableDefinition tableDefinition = null) {
            var fileName = Path.Combine(_filename, _dataFiles[sourceTableName]);
            var schemaFileName = Path.Combine(_filename, _schemaFiles[sourceTableName]);
            var binaryData = File.ReadAllBytes(fileName);
            var schema = File.ReadAllText(schemaFileName);
            var dataTable = ExtensionMethods.DataTableExtensions.FastDeserialize(binaryData, schema);
            if (tableDefinition != null) {
                var dataReader = new TableDefinitionDataReader(new DataTableReader(dataTable), tableDefinition);
                return dataReader;
            } else {
                return new DataTableReader(dataTable);
            }
        }
    }
}
