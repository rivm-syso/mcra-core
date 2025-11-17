using MCRA.Utils.ExtensionMethods;
using System.Data;
using System.Data.OleDb;
using System.Runtime.Versioning;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source reader for MS Access files.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class AccessDataFileReader : DataFileReader {

        private readonly OleDbConnection _connection;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename"></param>
        public AccessDataFileReader(string filename)
            : base(filename) {
            if (_connection == null) {
                _connection = new OleDbConnection();
                _connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + this.FullPath;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~AccessDataFileReader() {
            dispose(false);
        }

        /// <summary>
        /// Opens the reader connection.
        /// </summary>
        public override void Open() {
            if (_connection.State != ConnectionState.Open) {
                _connection.Open();
            }
        }

        /// <summary>
        /// Closes the reader connection.
        /// </summary>
        public override void Close() {
            if (_connection != null && _connection.State != ConnectionState.Closed) {
                _connection.Close();
            }
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTableNames() {
            var tableNames = new List<string>();
            try {
                string[] restrictions = new string[4];
                restrictions[3] = "Table";
                var userTables = _connection.GetSchema("Tables", restrictions);
                for (int i = 0; i < userTables.Rows.Count; i++) {
                    tableNames.Add(userTables.Rows[i][2].ToString());
                }
            } catch {
            }
            return tableNames;
        }

        /// <summary>
        /// Return datareader
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableName"></param>
        /// <returns></returns>
        public override IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName) {
            var tableAlias = $"{string.Join(", ", tableDefinition.Aliases)}, {string.Join(", ", tableDefinition.HiddenAliases)}";
            try {
                var tableNames = GetTableNames();
                sourceTableName = tableNames.SingleOrDefault(s => tableDefinition.AcceptsName(s));
            } catch (OleDbException ex) {
                var Hresult = ex.HResult;
                if (Hresult == -2147467259) {
                    throw new Exception("wrong MS Access version (higher than 97)");
                } else {
                    throw new Exception($"unknown error in table {tableAlias}");
                }
            } catch (Exception) {
                throw new Exception($"duplicate tables {tableAlias}");
            }
            if (sourceTableName != null) {
                var cmd = new OleDbCommand();
                cmd.CommandText = $"SELECT * FROM [{sourceTableName}];";
                cmd.Connection = _connection;
                return new CheckedDataTableReader(cmd.ExecuteReader(), tableDefinition);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Override.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        public override IDataReader GetDataReaderByName(string sheetName, TableDefinition tableDefinition = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Evaluates columns against table definitions, Excel format is less strict for integer and double definition.
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
                    var index = mappings[i];
                    var definition = columnDefinitions.ElementAt(i);
                    var acceptedFieldTypes = getAcceptedFieldTypes(definition.GetFieldType());
                    var fieldType = fieldTypes[index];
                    var fieldTypeName = definition.Required || fieldType == typeof(string)
                        ? fieldType.Name
                        : Nullable.GetUnderlyingType(fieldType).Name;

                    if (!acceptedFieldTypes.Contains(fieldTypeName)) {
                        throw new Exception($"Fieldtype of column {definition.Id} must be {definition.FieldType} ({fieldTypeName} is found))");
                    }
                }
            }
        }

        private static List<string> getAcceptedFieldTypes(FieldType fieldType) {
            return fieldType switch {
                FieldType.Undefined => _alphaNumeric.ToList(),// If undefined, assume alphanumeric
                FieldType.AlphaNumeric => _alphaNumeric.ToList(),
                FieldType.Numeric => _numericFieldTypes.ToList(),
                FieldType.Boolean => _booleanFieldTypes.ToList(),
                FieldType.Integer => _integerFieldTypes.ToList(),
                FieldType.DateTime => _dateTimeFieldTypes.ToList(),
                _ => throw new Exception($"Unknown field type {fieldType.GetDisplayName()}!"),
            };
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

        #region IDisposable Members

        protected override void dispose(bool disposing) {
            if (disposing == true) {
                Close();
                _connection.Dispose();
            }
        }

        #endregion

    }
}
