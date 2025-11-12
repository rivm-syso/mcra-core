using System.Data;
using MCRA.Utils.DataSourceReading.DataReaders;

namespace MCRA.Utils.DataSourceReading.DataSourceReaders {

    /// <summary>
    /// Encapsulates an IDataReader and adds a virtual column with a constant value
    /// in the front of the data table.
    /// </summary>
    public sealed class MappedColumnDataReader : DataReaderBase {

        private readonly int[] _columnMappings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnMappings"></param>
        public MappedColumnDataReader(IDataReader reader, int[] columnMappings) : base(reader) {
            _columnMappings = columnMappings;
        }

        /// <summary>
        /// Translate column index for internal reader using the column mappings
        /// </summary>
        /// <param name="i">Index for this reader</param>
        /// <returns>Translated index for the internal reader in the base class</returns>
        protected override int TranslateFieldIndex(int i) => _columnMappings[i];

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public override int FieldCount => _columnMappings.Length;

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override int GetOrdinal(string name) {
            var internalIndex = base.GetOrdinal(name);
            return Array.IndexOf(_columnMappings, internalIndex);
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the IDataReader.
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTable() {
            DataTable schemaTable = new DataTable();
            schemaTable.Columns.Add("ColumnName", typeof(string));
            schemaTable.Columns.Add("ColumnOrdinal", typeof(int));
            schemaTable.Columns.Add("ColumnSize", typeof(int));
            schemaTable.Columns.Add("DataType", typeof(Type));
            var internalTableSchema = base.GetSchemaTable();
            for (int i = 0; i < _columnMappings.Length; i++) {
                var row = internalTableSchema.Rows[i];
                schemaTable.Rows.Add(row["ColumnName"], i, row["ColumnSize"], row["DataType"]);
            }
            return schemaTable;
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override int GetValues(object[] values) {
            var internalValues = new object[FieldCount];
            base.GetValues(internalValues);
            for (int i = 1; i < FieldCount; i++) {
                values[i] = internalValues[i - 1];
            }
            return FieldCount;
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override bool IsDBNull(int i) {
            if (_columnMappings[i] < 0 || _columnMappings[i] >= base.FieldCount) {
                return true;
            }
            return base.IsDBNull(i);
        }
    }
}
