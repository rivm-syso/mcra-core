using System.Data;
using MCRA.Utils.DataSourceReading.DataReaders;

namespace MCRA.Utils.DataSourceReading.DataSourceReaders {

    /// <summary>
    /// Encapsulates an IDataReader, renaming the specified column with
    /// a new column name.
    /// </summary>
    /// <remarks>
    /// Constructor.
    /// </remarks>
    /// <param name="reader"></param>
    /// <param name="oldColumnName"></param>
    /// <param name="newColumnName"></param>
    public sealed class RenamedColumnDataReader(
        IDataReader reader,
        string oldColumnName,
        string newColumnName
    ) : DataReaderBase(reader) {

        private readonly string _oldColumnName = oldColumnName;
        private readonly string _newColumnName = newColumnName;

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetName(int i) {
            var name = base.GetName(i);
            if (name.Equals(_oldColumnName, StringComparison.InvariantCultureIgnoreCase)) {
                return _newColumnName;
            }
            return name;
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override int GetOrdinal(string name) {
            if (name.Equals(_newColumnName, StringComparison.InvariantCultureIgnoreCase)) {
                return base.GetOrdinal(_oldColumnName);
            }
            return base.GetOrdinal(name);
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
            var counter = 1;
            foreach (DataRow row in internalTableSchema.Rows) {
                string name = row["ColumnName"].ToString();
                if (!string.Equals(name, _oldColumnName, StringComparison.OrdinalIgnoreCase)) {
                    name = _newColumnName;
                }
                schemaTable.Rows.Add(name, counter, row["ColumnSize"], row["DataType"]);
                counter++;
            }
            return schemaTable;
        }
    }
}
