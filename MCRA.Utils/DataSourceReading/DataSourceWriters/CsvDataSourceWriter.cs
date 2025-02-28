using System.Data;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Utils.DataFileReading {
    /// <summary>
    /// CsvDataSourceWriter
    /// </summary>
    public class CsvDataSourceWriter : IDataSourceWriter, IDisposable {
        /// <summary>
        /// Directory to which the data files will be written.
        /// </summary>
        protected readonly DirectoryInfo _dataFolder;

        /// <summary>
        /// Constructor, must provide temp folder for writing the csv files.
        /// </summary>
        /// <param name="dataFolder"></param>
        public CsvDataSourceWriter(DirectoryInfo dataFolder) {
            _dataFolder = dataFolder;
            if (!_dataFolder.Exists) {
                _dataFolder.Create();
                _dataFolder.Refresh();
            }
        }

        /// <summary>
        /// Closes the writer.
        /// </summary>
        public virtual void Close() {
        }

        /// <summary>
        /// Opens the writer.
        /// </summary>
        public virtual void Open() {
            if (!_dataFolder.Exists) {
                throw new DirectoryNotFoundException("Data directory not found.");
            }
        }

        /// <summary>
        /// Write the data to a temporary file.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="tableDefinition"></param>
        public void Write(
            DataTable data,
            string destinationTableName,
            TableDefinition tableDefinition
        ) {
            try {
                var targetFileCsv = Path.Combine(_dataFolder.FullName, $"{destinationTableName}.csv");

                // Determine whether properties contain a file reference (a blob)
                var fileRefColumn = tableDefinition.ColumnDefinitions
                    .FirstOrDefault(r => r.GetFieldType() == FieldType.FileReference);

                if (fileRefColumn != null) {
                    var index = data.Columns
                        .Cast<DataColumn>()
                        .FirstIndexMatch(c => c.ColumnName == fileRefColumn.Id);
                    var rows = data.Rows.Cast<DataRow>().ToList();
                    foreach (var row in rows) {
                        var filePath = row.ItemArray[index].ToString();
                        var sourceFileName = Path.GetFileName(filePath);
                        var targetFileName = Path.Combine(_dataFolder.FullName, sourceFileName);
                        row[index] = sourceFileName;
                        File.Copy(filePath, targetFileName, true);
                    }
                }
                var orderBy = tableDefinition.ColumnDefinitions
                    .Where(cd => cd.OrderRank > 0)
                    .OrderBy(cd => cd.OrderRank)
                    .Select(cd => cd.Id);
                var append = File.Exists(targetFileCsv);
                data.ToCsv(targetFileCsv, orderBy: orderBy, append: append);
            } catch (Exception ex) {
                throw new Exception($"An error occured in table '{destinationTableName}': {ex}");
            }
        }

        /// <summary>
        /// Writes the source table to the destination table.
        /// </summary>
        /// <param name="sourceTableReader"></param>
        /// <param name="tableDefinition"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="progressState"></param>
        public void Write(
            IDataReader sourceTableReader,
            TableDefinition tableDefinition,
            string destinationTableName,
            ProgressState progressState = null
        ) {
            try {
                // Create in memory data table based on table definition
                var destinationTable = tableDefinition.CreateDataTable();

                // Get column mappings
                var mappings = tableDefinition.ColumnDefinitions.GetColumnMappings(sourceTableReader.GetColumnNames());

                // Write row-by-row using the column mappings
                while (sourceTableReader.Read()) {
                    var newRow = destinationTable.NewRow();
                    for (int i = 0; i < tableDefinition.ColumnDefinitions.Count; i++) {
                        if (mappings[i] > -1) {
                            newRow[i] = sourceTableReader.IsDBNull(mappings[i])
                                ? DBNull.Value
                                : sourceTableReader.GetValue(mappings[i]);
                        }
                    }
                    destinationTable.Rows.Add(newRow);
                }
                sourceTableReader.Close();

                // Write to output
                // For the unit test for PBK model definitions there is no stream, a copy is made
                Write(destinationTable, destinationTableName, tableDefinition);
            } catch (Exception ex) {
                throw new Exception($"An error occured in table '{destinationTableName}': {ex}");
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose implementation.
        /// </summary>
        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing) {
            if (disposing == true) {
                Close();
            }
        }
        #endregion
    }
}
