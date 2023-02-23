using MCRA.Utils.ProgressReporting;
using System.Data;

namespace MCRA.Utils.DataFileReading {
    public class DataTableDataSourceWriter : IDataSourceWriter {

        /// <summary>
        /// Contains the data tables that were read.
        /// </summary>
        public Dictionary<string, DataTable> DataTables { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataTableDataSourceWriter() {
            DataTables = new Dictionary<string, DataTable>();
        }

        /// <summary>
        /// Closes the writer.
        /// </summary>
        public void Close() {
            // nothing to do here
        }

        /// <summary>
        /// Opens the writer.
        /// </summary>
        public void Open() {
            // nothing to do here
        }

        /// <summary>
        /// Write the data to the data tables dictionary.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="tableDefinition"></param>
        public void Write(
            DataTable data,
            string destinationTableName,
            TableDefinition tableDefinition
        ) {
            if (DataTables.TryGetValue(destinationTableName, out var dataTable)) {
                dataTable.Merge(data);
            } else {
                DataTables[destinationTableName] = data;
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

                if (DataTables.TryGetValue(destinationTableName, out var dataTable)) {
                    dataTable.Merge(destinationTable);
                } else {
                    DataTables[destinationTableName] = destinationTable;
                }

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
