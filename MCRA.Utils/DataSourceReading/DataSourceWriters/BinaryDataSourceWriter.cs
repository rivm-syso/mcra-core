using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using System.Data;

namespace MCRA.Utils.DataFileReading {
    /// <summary>
    /// BinaryDataSourceWriter
    /// </summary>
    public class BinaryDataSourceWriter : IDataSourceWriter, IDisposable{
        /// <summary>
        /// Directory to which the data files will be written.
        /// </summary>
        protected readonly DirectoryInfo _dataFolder;

        /// <summary>
        /// Constructor, must provide temp folder for writing the csv files.
        /// </summary>
        /// <param name="dataFolder"></param>
        public BinaryDataSourceWriter(DirectoryInfo dataFolder) {
            _dataFolder = dataFolder;
            if (!_dataFolder.Exists) {
                _dataFolder.Create();
                _dataFolder.Refresh();
            }
        }

        /// <summary>
        /// Constructor, initializes with csv temp folder string.
        /// </summary>
        /// <param name="dataPath"></param>
        public BinaryDataSourceWriter(string dataPath) {
            _dataFolder = Directory.CreateDirectory(dataPath);
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
                var targetFile = Path.Combine(_dataFolder.FullName, $"{destinationTableName}.bin");
                var schemaFile = Path.Combine(_dataFolder.FullName, $"{destinationTableName}.def");

                //get the desired field ordering based on the column definition's
                //order ranks based on columns that have an OrderRank > 0
                var orderBy = tableDefinition.ColumnDefinitions
                    .Where(cd => cd.OrderRank > 0)
                    .OrderBy(cd => cd.OrderRank)
                    .Select(cd => cd.Id);

                //set the table name explicitly
                if(string.IsNullOrEmpty(data.TableName)) {
                    data.TableName = tableDefinition.TargetDataTable;
                }
                var bytes = data.FastSerialize(orderBy, out var schema);

                File.WriteAllBytes(targetFile, bytes);
                File.WriteAllText(schemaFile, schema);
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
