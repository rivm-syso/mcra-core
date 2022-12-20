using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source reader for reading zipped csv file collections.
    /// </summary>
    public class ZipCsvFileReader: DataFileReader {

        private ZipArchive _zipArchive;

        private Dictionary<string, string> _zipEntries = new();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName"></param>
        public ZipCsvFileReader(string fileName): base(fileName) {
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~ZipCsvFileReader() {
            dispose(false);
        }

        /// <summary>
        /// Closes the reader connection.
        /// </summary>
        public override void Close() {
            if (_zipArchive != null) {
                _zipArchive.Dispose();
                _zipArchive = null;
            }
        }

        /// <summary>
        /// Opens the reader connection.
        /// </summary>
        public override void Open() {
            if (_zipArchive == null) {
                if (!File.Exists(FullPath)) {
                    throw new Exception($"Data source zip file {FullPath} not found.");
                }
                _zipArchive = ZipFile.OpenRead(FullPath);
                _zipEntries = _zipArchive.Entries
                    .Where(f => f.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    .Select(f => f.FullName)
                    .ToDictionary(s => Path.GetFileNameWithoutExtension(s), StringComparer.OrdinalIgnoreCase);
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
                Debug.WriteLine("Exception enumerating files in archive.\n" + ex.ToString());
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
                throw new Exception($"Multiple data tables found for {tableDefinition.Name}");
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
    }
}
