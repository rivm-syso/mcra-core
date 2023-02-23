using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using System.Data;

namespace MCRA.Data.Raw.Converters {

    /// <summary>
    /// Data source writer that writes the data sources to csv files
    /// in a specified csv folder.
    /// </summary>
    public class RecodingDataSourceWriter : IDataSourceWriter, IDisposable {

        private IDataSourceWriter _internalDataSourceWriter;
        private readonly EntityCodeConversionsCollection[] _codeConversions;

        public RecodingDataSourceWriter(IDataSourceWriter internalDataSourceWriter, params EntityCodeConversionsCollection[] codeConversions) {
            _internalDataSourceWriter = internalDataSourceWriter;
            _codeConversions = codeConversions;
        }

        public void Close() {
            _internalDataSourceWriter.Close();
        }

        public void Open() {
            _internalDataSourceWriter.Open();
        }

        public void Write(
            DataTable data,
            string destinationTableName,
            TableDefinition tableDefinition = null
        ) {
            var headers = data.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            var columnConversions = getCodeConversions(tableDefinition, destinationTableName, headers);
            if (columnConversions?.Any() ?? false) {
                var converted = data.Copy();
                foreach (var columnConversion in columnConversions) {
                    foreach (DataRow row in converted.Rows) {
                        var origValue = row[columnConversion.ColumnName]?.ToString();
                        if (!string.IsNullOrEmpty(origValue) && columnConversion.Conversions.TryGetValue(origValue, out var newValue)) {
                            row[columnConversion.ColumnName] = newValue;
                        }
                    }
                }
                _internalDataSourceWriter.Write(converted, destinationTableName, tableDefinition);
            } else {
                _internalDataSourceWriter.Write(data, destinationTableName, tableDefinition);
            }
        }

        public void Write(
            IDataReader sourceTableReader,
            TableDefinition tableDefinition,
            string destinationTableName,
            ProgressState progressState = null
        ) {
            var headers = sourceTableReader.GetColumnNames().ToArray();
            var columnConversions = getCodeConversions(tableDefinition, destinationTableName, headers);
            IDataReader dataReader = sourceTableReader;
            if (columnConversions?.Any() ?? false) {
                foreach (var columnConversion in columnConversions) {
                    dataReader = new RecodingDataReader(dataReader, columnConversion.ColumnName, columnConversion.Conversions);
                }
            }
            _internalDataSourceWriter.Write(dataReader, tableDefinition, destinationTableName, progressState);
        }

        private List<(string ColumnName, Dictionary<string, string> Conversions)> getCodeConversions(
            TableDefinition tableDefinition,
            string destinationTableName,
            string[] headers
        ) {
            var conversionsDictionary = _codeConversions.ToDictionary(r => r.IdEntity, StringComparer.OrdinalIgnoreCase);
            if (tableDefinition != null) {
                var primaryKeyMappings = tableDefinition.ColumnDefinitions
                    .Where(cd => cd.IsPrimaryKey && conversionsDictionary.ContainsKey(tableDefinition.Id))
                    .Select(cd => new {
                        ColumnDefinition = cd,
                        Conversions = conversionsDictionary[tableDefinition.Id]
                    })
                    .SelectMany(
                        r => headers.Where(h => r.ColumnDefinition.AcceptsHeader(h, false)),
                        (r, h) =>
                            (
                                ColumnName: h,
                                Conversions: r.Conversions.ConversionTuples
                                    .ToDictionary(ct => ct.OriginalCode, ct => ct.ToCode, StringComparer.OrdinalIgnoreCase)
                            )
                        )
                    .ToList();

                var foreignKeyMappings = tableDefinition.ColumnDefinitions
                    .Where(cd => cd.ForeignKeyTables.Any(fk => conversionsDictionary.ContainsKey(fk)))
                    .Select(cd => new {
                        ColumnDefinition = cd,
                        Conversions = conversionsDictionary[cd.ForeignKeyTables.First(r => conversionsDictionary.ContainsKey(r))]
                    })
                    .SelectMany(
                        r => headers.Where(h => r.ColumnDefinition.AcceptsHeader(h, false)),
                        (r, h) =>
                            (
                                ColumnName: h,
                                Conversions: r.Conversions.ConversionTuples
                                    .ToDictionary(ct => ct.OriginalCode, ct => ct.ToCode, StringComparer.OrdinalIgnoreCase)
                            )
                        )
                    .ToList();

                var result = primaryKeyMappings;
                result.AddRange(foreignKeyMappings);

                return result;
            } else {
                return null;
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
                _internalDataSourceWriter.Dispose();
            }
        }

        #endregion
    }
}
