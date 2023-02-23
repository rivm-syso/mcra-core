using System.Data;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Subclass of DataFileReader which stores DataTables in memory to use as raw data sources for bulk copy operations
    /// </summary>
    public class RawDataTableReader : IDataSourceReader {

        private readonly IDictionary<string, DataTable> _dataTables;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataTables">Dictionary of raw data tables as DataTable objects indexed by raw table name</param>
        public RawDataTableReader(params DataTable[] dataTables) {
            _dataTables = dataTables.ToDictionary(dt => dt.TableName);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataTables">Dictionary of raw data tables as DataTable objects indexed by raw table name</param>
        public RawDataTableReader(IDictionary<string, DataTable> dataTables) {
            _dataTables = dataTables;
        }

        /// <summary>
        /// Opens the data file for reading.
        /// </summary>
        public void Open() {
            // No actions required
        }

        /// <summary>
        /// Closes the data file reader.
        /// </summary>
        public void Close() {
            // No actions required
        }

        /// <summary>
        /// Gets the names of all the tables in the data source
        /// </summary>
        /// <returns></returns>
        public List<string> GetTableNames() {
            return _dataTables?.Keys.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Implements <see cref="IDataSourceReader.HasDataForTableDefinition(TableDefinition)"/>.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public bool HasDataForTableDefinition(TableDefinition tableDefinition) {
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
        public IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName) {
            if (_dataTables.TryGetValue(tableDefinition.Id, out var table)) {
                sourceTableName = tableDefinition.Id;
                return table.CreateDataReader();
            } else {
                sourceTableName = null;
            }
            return null;
        }

        /// <summary>
        /// Returns a data reader based on the provided table definition.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition) {
            return GetDataReaderByDefinition(tableDefinition, out var sourceTableName);
        }

        /// <summary>
        /// Returns a data reader based on the provided table name.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public IDataReader GetDataReaderByName(string sheetName, TableDefinition tableDefinition) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for the data table specified by the table definition and parses it
        /// into the data table according to the specified table definition.
        /// </summary>
        /// <param name="sourceTableReader"></param>
        /// <param name="columnDefinitions"></param>
        public DataTable ToDataTable(IDataReader sourceTableReader, ICollection<ColumnDefinition> columnDefinitions) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for the data table specified by the table definition and parses it
        /// into the list of objects according to the specified table definition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public List<T> ReadDataTable<T>(TableDefinition tableDefinition) where T : new() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Evaluates columns against table definitions.
        /// </summary>
        /// <param name="columnDefinitions"></param>
        /// <param name="sourceTableReader"></param>
        public void ValidateSourceTableColumns(ICollection<ColumnDefinition> columnDefinitions, IDataReader sourceTableReader) {
            var columnNames = new List<string>();
            var fieldTypes = new List<Type>();
            for (int i = 0; i < sourceTableReader.FieldCount; i++) {
                columnNames.Add(sourceTableReader.GetName(i).ToLower());
                fieldTypes.Add(sourceTableReader.GetFieldType(i));
            }
            var mappings = columnDefinitions.GetColumnMappings(columnNames);
            for (int i = 0; i < columnDefinitions.Count; i++) {
                if (mappings[i] > -1) {
                    var index = mappings[i];
                    var definition = columnDefinitions.ElementAt(i);
                    var acceptedFieldTypes = getAcceptedFieldTypes(definition.GetFieldType());
                    if (!acceptedFieldTypes.Contains(fieldTypes[index].Name)) {
                        throw new Exception($"Field type of column {definition.Id} must be {definition.FieldType} ({fieldTypes[index].Name} is found))");
                    }
                }
            }
        }

        /// <summary>
        /// Validation of the source table w.r.t. the tableDefinition.
        /// Throws an exception when the validation fails.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableReader"></param>
        public virtual void ValidateSourceTableColumns(TableDefinition tableDefinition, IDataReader sourceTableReader) {
            ValidateSourceTableColumns(tableDefinition.ColumnDefinitions, sourceTableReader);
        }

        private static List<string> getAcceptedFieldTypes(FieldType fieldType) {
            switch (fieldType) {
                case FieldType.Undefined:
                    // If undefined, assume alphanumeric
                    return _alphaNumeric.ToList();
                case FieldType.AlphaNumeric:
                    return _alphaNumeric.ToList();
                case FieldType.Numeric:
                    return _numericFieldTypes.ToList();
                case FieldType.Boolean:
                    return _booleanFieldTypes.ToList();
                case FieldType.Integer:
                    return _integerFieldTypes.ToList();
                case FieldType.DateTime:
                    return _dateTimeFieldTypes.ToList();
                default:
                    throw new Exception($"Unknown field type {fieldType.GetDisplayName()}!");
            }
        }

        #region Known/accepted Field Types

        private static readonly HashSet<string> _alphaNumeric = new(StringComparer.OrdinalIgnoreCase) {
            "String",
            "Double",
            "Int16",
            "Int32",
            "Int64",
            "Integer",
            "Decimal",
            "Float"
        };

        private static readonly HashSet<string> _numericFieldTypes = new(StringComparer.OrdinalIgnoreCase) {
            "Double",
            "Float",
            "Decimal",
            "Int16",
            "Int32",
            "Int64",
            "Integer"
        };

        private static readonly HashSet<string> _integerFieldTypes = new(StringComparer.OrdinalIgnoreCase) {
            "Integer",
            "Int16",
            "Int32",
            "Int64",
        };

        private static readonly HashSet<string> _booleanFieldTypes = new(StringComparer.OrdinalIgnoreCase) {
            "Boolean",
            "Byte",
            "Int16",
            "Int32",
            "Int64",
            "Integer",
            "String"
        };

        private static readonly HashSet<string> _dateTimeFieldTypes = new(StringComparer.OrdinalIgnoreCase) {
            "DateTime"
        };

        #endregion

        #region Disposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RawDataTableReader()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
