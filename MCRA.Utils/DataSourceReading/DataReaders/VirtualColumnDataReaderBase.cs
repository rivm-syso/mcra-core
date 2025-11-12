using System.Data;
using MCRA.Utils.DataSourceReading.DataReaders;

namespace MCRA.Utils.DataSourceReading.DataSourceReaders {

    /// <summary>
    /// Encapsulates an IDataReader and adds a virtual column with a constant value
    /// in the front of the data table.
    /// </summary>
    /// <remarks>
    /// Constructor.
    /// </remarks>
    /// <param name="reader"></param>
    /// <param name="virtualColumnName"></param>
    /// <param name="virtualColumnValue"></param>
    public abstract class VirtualColumnDataReaderBase<T>(
        IDataReader reader,
        string virtualColumnName,
        T virtualColumnValue
    ) : DataReaderBase(reader) {

        protected readonly string _virtualColumnName = virtualColumnName;
        protected readonly T _virtualColumnValue = virtualColumnValue;

        /// <summary>
        /// Override function to map an index of this class
        /// to the internal reader's index.
        /// In this case the reader adds a virtual column at position 0
        /// So all columns are shifted 1 to the right
        /// The internal reader's index should then be mapped to i - 1
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">
        /// A value of 0 can't logically be mapped to the internal reader
        /// Solution: override the base function providing an implementation
        /// for the caller
        /// </exception>
        protected override int TranslateFieldIndex(int i) {
            return i == 0
                ? throw new NotImplementedException()
                : i - 1;
        }

        /// <summary>
        /// Get the name of the column, in case the index is 0, get the virtual column name
        /// otherwise use the base function
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Name of the column</returns>
        public override string GetName(int i) => i == 0 ? _virtualColumnName : base.GetName(i);

        /// <summary>
        /// Get the data type name of the column, in case the index is 0, get the virtual column data type name
        /// otherwise use the base function
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Data type name of the column</returns>
        public override string GetDataTypeName(int i) => i == 0 ? typeof(T).Name : base.GetDataTypeName(i);

        /// <summary>
        /// Get the ordinal of the column, in case it's the virtual column name
        /// return 0 otherwise the internal reader's ordinal incremented by 1
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Data type name of the column</returns>
        public override int GetOrdinal(string name) {
            return name.Equals(_virtualColumnName, StringComparison.InvariantCultureIgnoreCase)
                ? 0
                : base.GetOrdinal(name) + 1;
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public override int FieldCount => base.FieldCount + 1;

        /// <summary>
        /// Gets the Type information corresponding to the type of Object that would be returned from GetValue.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override Type GetFieldType(int i) => i == 0 ? typeof(T) : base.GetFieldType(i);

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
            schemaTable.Rows.Add(_virtualColumnName, 0, 2048, typeof(T));
            var internalTableSchema = base.GetSchemaTable();
            var counter = 1;
            foreach (DataRow row in internalTableSchema.Rows) {
                schemaTable.Rows.Add(row["ColumnName"], counter, row["ColumnSize"], row["DataType"]);
                counter++;
            }
            return schemaTable;
        }

        /// <summary>
        /// Get the value of the column, in case it's the virtual column index 0
        /// return the stored value otherwise the internal reader's value
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Object value of the virtual index i</returns>
        public override object GetValue(int i) {
            return i == 0 ? _virtualColumnValue : base.GetValue(i);
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override int GetValues(object[] values) {
            values[0] = _virtualColumnValue;
            var internalValues = new object[FieldCount - 1];
            base.GetValues(internalValues);
            for (int i = 1; i < FieldCount; i++) {
                values[i] = internalValues[i - 1];
            }
            return FieldCount;
        }
    }
}
