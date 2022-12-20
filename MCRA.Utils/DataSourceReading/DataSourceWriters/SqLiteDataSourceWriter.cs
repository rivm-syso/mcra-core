using MCRA.Utils.ProgressReporting;
using Microsoft.Data.Sqlite;
using System.Data;

namespace MCRA.Utils.DataFileReading {

    public sealed class SqLiteDataSourceWriter : IDataSourceWriter, IDisposable {

        private readonly string _sqliteDbFileName;
        private readonly SqliteConnection _connection;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sqliteDbFileName"></param>
        public SqLiteDataSourceWriter(string sqliteDbFileName) {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            _sqliteDbFileName = sqliteDbFileName;
            _connection = new SqliteConnection($"Data Source={_sqliteDbFileName};");
        }

        /// <summary>
        /// Opens the reader for reading the data.
        /// </summary>
        public void Open() {
            if (_connection.State != ConnectionState.Open) {
                _connection.Open();
            }
        }

        /// <summary>
        /// Closes the data reader.
        /// </summary>
        public void Close() {
            if (_connection != null && _connection.State != ConnectionState.Closed) {
                _connection.Close();
            }
        }

        /// <summary>
        /// Writes the data table to the specified destination table.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="tableDefinition"></param>
        public void Write(DataTable dataTable, string destinationTableName, TableDefinition tableDefinition) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Runs the given sql statement.
        /// </summary>
        /// <param name="sql"></param>
        public void RunSql(string sql) {
            using (var command = new SqliteCommand(sql, _connection)) {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Returns a reader
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqliteCommand CreateSQLiteCommand(string sql) {
            return new SqliteCommand(sql, _connection);
        }

        /// <summary>
        /// Writes the source table specified by the table definition to the specified destination table.
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
            var columnDefinitions = tableDefinition.ColumnDefinitions;

            var fieldNames = sourceTableReader.GetColumnNames();
            var mappings = columnDefinitions.GetColumnMappings(fieldNames);

            var outputColumns = new List<ColumnDefinition>();
            var valueFields = new List<string>();
            for (int i = 0; i < tableDefinition.ColumnDefinitions.Count; i++) {
                if (mappings[i] > -1) {
                    var columnDefinition = tableDefinition.ColumnDefinitions[i];
                    var index = mappings[i];
                    outputColumns.Add(columnDefinition);
                    valueFields.Add(fieldNames[mappings[i]]);
                }
            }

            var outputFieldsString = string.Join(", ", outputColumns.Select(c => $"{c.Id}"));
            var valueFieldsString = string.Join(", ", outputColumns.Select(c => $"@{c.Id}"));

            using (var command = _connection.CreateCommand()) {
                command.CommandText = $"INSERT INTO {destinationTableName} ({outputFieldsString}) VALUES ({valueFieldsString})";
                command.Parameters.AddRange(outputColumns.Where(c => c != null).Select(c => new SqliteParameter($"@{c.Id}", getSqliteParameterType(c))).ToArray());

                command.Transaction = _connection.BeginTransaction();
                var transactionRecordCounter = 0;
                while (sourceTableReader.Read()) {
                    for (int i = 0; i < tableDefinition.ColumnDefinitions.Count; i++) {
                        var columnDefinition = tableDefinition.ColumnDefinitions[i];
                        if (mappings[i] > -1) {
                            command.Parameters[$"@{columnDefinition.Id}"].Value = sourceTableReader.IsDBNull(mappings[i]) ? DBNull.Value : sourceTableReader[mappings[i]];
                        }
                    }

                    command.ExecuteNonQuery();
                    if (++transactionRecordCounter % 92551 == 0) {
                        // If cancelled, stop writing, break out of the loop
                        if (progressState?.CancellationToken.IsCancellationRequested ?? false) {
                            break;
                        }
                        command.Transaction.Commit();
                        progressState?.Update($"Copying records: {transactionRecordCounter / 1000}K records copied.");
                        command.Transaction = _connection.BeginTransaction();
                    }
                }
                command.Transaction.Commit();
                command.Transaction.Dispose();
            }
        }

        private static DbType getSqliteParameterType(ColumnDefinition colDef) {
            switch (colDef.GetFieldType()) {
                case FieldType.Numeric:
                    return DbType.Double;
                case FieldType.Integer:
                    return DbType.Int32;
                case FieldType.DateTime:
                    return DbType.DateTime2;
                case FieldType.AlphaNumeric:
                    return DbType.String;
                case FieldType.Undefined:
                    return DbType.String;
                default:
                    throw new NotImplementedException();
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
                _connection.Dispose();
            }
        }

        #endregion

    }
}
