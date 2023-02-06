using MCRA.Utils.ExtensionMethods;
using System.Data;
using System.Security.Cryptography;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Base class of a data reader. Intended to read from various types
    /// of data sources in a uniform way.
    /// </summary>
    public abstract class DataSourceReaderBase : IDataSourceReader {

        private bool _disposed;

        /// <summary>
        /// Opens the data file for reading.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Closes the data file reader.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Implements <see cref="IDataSourceReader.HasDataForTableDefinition(TableDefinition)"/>.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public virtual bool HasDataForTableDefinition(TableDefinition tableDefinition) {
            var tableNames = GetTableNames();
            return tableNames.Any(s => tableDefinition.AcceptsName(s));
        }

        /// <summary>
        /// Searches the data source for the target table.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableName"></param>
        /// <returns>A reader for the target table. If the table does not exist in the data source, null is returned.</returns>
        /// <remarks>
        /// Faster than GetDataTableByDefinition, because it does not load any data into memory, it only provides an open pipeline to the underlying
        /// data file. Use this method when bulkcopying data whenever possible.
        /// </remarks>
        public abstract IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName);

        /// <summary>
        /// Searches the data source for the target table.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <returns>A reader for the target table. If the table does not exist in the data source, null is returned.</returns>
        /// <remarks>
        /// Faster than GetDataTableByDefinition, because it does not load any data into memory, it only provides an open pipeline to the underlying
        /// data file. Use this method when bulkcopying data whenever possible.
        /// </remarks>
        public IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition) {
            return GetDataReaderByDefinition(tableDefinition, out _);
        }

        /// <summary>
        /// Searches for the data table with the specified name and returns a data reader for this table.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public abstract IDataReader GetDataReaderByName(string sheetName, TableDefinition tableDefinition);

        /// <summary>
        /// Gets the names of all the tables in the data source
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetTableNames();

        /// <summary>
        /// Validation of the source table w.r.t. the tableDefinition.
        /// Throws an exception when the validation fails.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableReader"></param>
        public virtual void ValidateSourceTableColumns(TableDefinition tableDefinition, IDataReader sourceTableReader) {
            ValidateSourceTableColumns(tableDefinition.ColumnDefinitions, sourceTableReader);
        }

        /// <summary>
        /// Validation of whether the source table reader complies to the column definitions.
        /// Throws an exception when the validation fails.
        /// </summary>
        /// <param name="columnDefinitions"></param>
        /// <param name="sourceTableReader"></param>
        public abstract void ValidateSourceTableColumns(ICollection<ColumnDefinition> columnDefinitions, IDataReader sourceTableReader);

        /// <summary>
        /// <summary>
        /// Searches for the data table specified by the table definition and parses it
        /// into the data table according to the specified table definition.
        /// </summary>
        /// <param name="sourceTableReader"></param>
        /// <param name="columnDefinitions"></param>
        public DataTable ToDataTable(IDataReader sourceTableReader, ICollection<ColumnDefinition> columnDefinitions) {
            // Create a temporary data table based on the columns in the columnDefinitions parameter
            var colDefs = columnDefinitions.ToList();
            var tableDef = new TableDefinition { ColumnDefinitions = colDefs };
            var dataTable = tableDef.CreateDataTable();

            //create a mapping array which maps the source column ordinals to the
            //output table's column ordinals, add 1 column for the 'idDataSource'
            var mappings = columnDefinitions.GetColumnMappings(sourceTableReader.GetColumnNames());

            // Fill table using the column mapping array
            while (sourceTableReader.Read()) {
                var row = dataTable.NewRow();
                for (int i = 0; i < mappings.Length; i++) {
                    if (mappings[i] > -1) {
                        row[i] = sourceTableReader.GetValue(mappings[i]);
                    }
                }
                dataTable.Rows.Add(row);
            }
            dataTable.AcceptChanges();
            return dataTable;
        }

        /// <summary>
        /// Reads the data table based on the provided target property mappings and returns a list of
        /// objects as determined by the generic object type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableDefinition"></param>
        public List<T> ReadDataTable<T>(
            TableDefinition tableDefinition
        ) where T : new() {
            using var sourceTableReader = GetDataReaderByDefinition(tableDefinition);
            var records = sourceTableReader.ReadRecords<T>(tableDefinition);
            return records;
        }

        /// <summary>
        /// Diagnostics method: prints the data table for the specified table definition.
        /// </summary>
        /// <param name="tableDefinition"></param>
        public void TracePrintDataTable(TableDefinition tableDefinition) {
            using var sourceTableReader = GetDataReaderByDefinition(tableDefinition);
            var dataTable = new DataTable();
            dataTable.Load(sourceTableReader);
            var tab = dataTable.ConvertDataTableToString();
            System.Diagnostics.Trace.Write(tab);
        }

        /// <summary>
        /// Calculates the SHA-256 hash of the file with the provided filename and returns
        /// it as a base64 encoded string.
        /// </summary>
        /// <param name="fileName">File name of file to calculate the hash for</param>
        /// <returns></returns>
        public static string CalculateFileHashBase64(string fileName) {
            if (!File.Exists(fileName)) {
                return string.Empty;
            }
            var hasher = SHA256.Create();
            byte[] hashValue;
            using (Stream s = File.OpenRead(fileName)) {
                hashValue = hasher.ComputeHash(s);
            }
            return Convert.ToBase64String(hashValue);
        }

        #region IDisposable

        /// <summary>
        /// Implementation of dispose method for IDisposable.
        /// </summary>
        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Part of standard dispose pattern, can be overriden.
        /// </summary>
        protected virtual void dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                Close();
            }

            _disposed = true;
        }
        #endregion

    }
}
