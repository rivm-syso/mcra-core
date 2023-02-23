using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using System.Data;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source writer that writes the data sources to csv files
    /// in a specified csv folder.
    /// </summary>
    public class CsvFileDataSourceWriter : IDataSourceWriter, IDisposable {

        /// <summary>
        /// Directory to which the data files will be written.
        /// </summary>
        protected readonly DirectoryInfo _csvDirectory;

        /// <summary>
        /// Constructor, must provide temp folder for writing the csv files.
        /// </summary>
        /// <param name="csvDirectory"></param>
        public CsvFileDataSourceWriter(DirectoryInfo csvDirectory) {
            _csvDirectory = csvDirectory;
        }

        /// <summary>
        /// Constructor, initializes with csv temp folder string.
        /// </summary>
        /// <param name="csvDirectoryPath"></param>
        public CsvFileDataSourceWriter(string csvDirectoryPath) {
            _csvDirectory = Directory.CreateDirectory(csvDirectoryPath);
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
            if (!_csvDirectory.Exists) {
                throw new DirectoryNotFoundException("CSV directory not found.");
            }
        }

        /// <summary>
        /// Write the data to a temporary CSV file.
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
                var targetFile = Path.Combine(_csvDirectory.FullName, $"{destinationTableName}.csv");
                var append = File.Exists(targetFile);
                data.ToCsv(targetFile, append:append);
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

                // Write to output
                Write(destinationTable, destinationTableName, tableDefinition);

                sourceTableReader.Close();
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
