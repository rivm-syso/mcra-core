using ExcelDataReader;
using System.Data;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source reader for MS Excel files.
    /// </summary>
    public sealed class ExcelFileReader : DataFileReader {

        private IExcelDataReader _reader;
        private List<string> _sheetNames = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename"></param>
        public ExcelFileReader(string filename)
            : base(filename) {
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~ExcelFileReader() {
            dispose(false);
        }

        /// <summary>
        /// Opens the reader.
        /// </summary>
        public override void Open() {
            if (_reader == null) {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                var fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                _reader = Path.GetExtension(_filename).ToLower() == ".xlsx"
                        ? ExcelReaderFactory.CreateOpenXmlReader(fs)
                        : ExcelReaderFactory.CreateBinaryReader(fs);
            }
        }

        /// <summary>
        /// Closes the reader.
        /// </summary>
        public override void Close() {
            if (_reader != null) {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTableNames() {
            checkOpenReader();
            if (_sheetNames == null) {
                _sheetNames = new List<string>();
                try {
                    _reader.Reset();
                    do {
                        _sheetNames.Add(_reader.Name);
                    } while (_reader.NextResult());
                } catch {
                    //suppress exceptions
                } finally {
                    _reader.Reset();
                }
            }
            return _sheetNames;
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableName"></param>
        /// <returns></returns>
        public override IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName) {
            var tableNames = GetTableNames().Where(s => !string.IsNullOrEmpty(s)).ToList();
            sourceTableName = tableNames.FirstOrDefault(s => tableDefinition.AcceptsName(trimSheetName(s)));
            return getDataReaderForSheet(sourceTableName, tableDefinition);
        }

        /// <summary>
        /// Override: returns the data reader for the sheet with the specified name.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public override IDataReader GetDataReaderByName(string sheetName, TableDefinition tableDefinition = null) {
            var sourceTableName = GetTableNames().FirstOrDefault(r => trimSheetName(r) == sheetName);
            return getDataReaderForSheet(sourceTableName, tableDefinition);
        }

        /// <summary>
        /// Validation of whether the source table reader complies to the column definitions.
        /// Throws an exception when the validation fails.
        /// </summary>
        /// <param name="columnDefinitions"></param>
        /// <param name="sourceTableReader"></param>
        public override void ValidateSourceTableColumns(ICollection<ColumnDefinition> columnDefinitions, IDataReader sourceTableReader) {
            var columnNames = sourceTableReader.GetColumnNames();
            var fieldTypes = new List<Type>();
            for (int i = 0; i < sourceTableReader.FieldCount; i++) {
                fieldTypes.Add(sourceTableReader.GetFieldType(i));
            }
            var mappings = columnDefinitions.GetColumnMappings(columnNames);
            for (int i = 0; i < columnDefinitions.Count; i++) {
                if (mappings[i] > -1) {
                    // TODO: check field type
                }
            }
        }

        /// <summary>
        /// Returns the data reader for the sheet with the specified name.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        private IDataReader getDataReaderForSheet(string sheetName, TableDefinition tableDefinition = null) {
            if (!string.IsNullOrEmpty(sheetName)) {
                try {
                    var fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var newSheetReader = Path.GetExtension(_filename).ToLower() == ".xlsx"
                        ? ExcelReaderFactory.CreateOpenXmlReader(fs)
                        : ExcelReaderFactory.CreateBinaryReader(fs);
                    newSheetReader.Reset();
                    while (!newSheetReader.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase)) {
                        newSheetReader.NextResult();
                    }
                    if (tableDefinition != null) {
                        var reader = new CheckedDataTableReader(newSheetReader, tableDefinition, true, true);
                        return reader;
                    } else {
                        return newSheetReader;
                    }
                } catch (Exception ex) {
                    throw new Exception($"Failed to read sheet {sheetName} of excel file {_filename}.", ex);
                }
            }
            return null;
        }

        private void checkOpenReader() {
            if (_reader?.IsClosed ?? true) {
                _reader?.Dispose();
                _reader = null;
                _sheetNames = null;
                Open();
            }
        }

        private static string trimSheetName(string sheetName) {
            return sheetName?.Trim('\'').TrimEnd('$') ?? string.Empty;
        }

        #region IDisposable Members

        protected override void dispose(bool disposing) {
            if (disposing && _reader != null) {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }
        }
        #endregion
    }
}
