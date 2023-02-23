using System.Data;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source reader interface. Intended to read from various types
    /// of data sources in a uniform way.
    /// </summary>
    public interface IDataSourceReader : IDisposable {

        /// <summary>
        /// Opens the reader for reading the data.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the data reader.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the names of all the tables in the data source.
        /// </summary>
        /// <returns></returns>
        List<string> GetTableNames();

        /// <summary>
        /// Returns whether the reader has table containing data of the specified.
        /// table definition.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        bool HasDataForTableDefinition(TableDefinition tableDefinition);

        /// <summary>
        /// Searches the data source for the target table.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <returns>A reader for the target table. If the table does not exist in the data source, null is returned.</returns>
        /// <remarks>
        /// Faster than GetDataTableByDefinition, because it does not load any data into memory, it only provides an open pipeline to the underlying
        /// data file. Use this method when bulkcopying data whenever possible.
        /// </remarks>
        IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition);

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
        IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName);

        /// <summary>
        /// Searches for the data table with the specified name and returns a data reader for this table.
        /// If a table definition is provided, the data reader will be specific for reading tables of the
        /// provided definition and will be of type <see cref="TableDefinitionDataReader"/>.
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="tableDefinition"></param>
        /// <returns></returns>
        IDataReader GetDataReaderByName(string sheetName, TableDefinition tableDefinition = null);

        /// <summary>
        /// Searches for the data table specified by the table definition and parses it
        /// into the data table according to the specified table definition.
        /// Additional property mappings can be specified using the property mapping dictionary.
        /// </summary>
        /// <param name="sourceTableReader"></param>
        /// <param name="columnDefinitions"></param>
        DataTable ToDataTable(IDataReader sourceTableReader, ICollection<ColumnDefinition> columnDefinitions);

        /// <summary>
        /// Searches for the data table specified by the table definition and parses it
        /// into the list of objects according to the specified table definition.
        /// Additional property mappings can be specified using the property mapping dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableDefinition"></param>
        List<T> ReadDataTable<T>(TableDefinition tableDefinition) where T : new();

        /// <summary>
        /// Validation of the source table w.r.t. the tableDefinition.
        /// Throws an exception when the validation fails.
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <param name="sourceTableReader"></param>
        void ValidateSourceTableColumns(TableDefinition tableDefinition, IDataReader sourceTableReader);

        /// <summary>
        /// Validation of whether the source table reader complies to the column definitions.
        /// Throws an exception when the validation fails.
        /// </summary>
        /// <param name="columnDefinitions"></param>
        /// <param name="sourceTableReader"></param>
        void ValidateSourceTableColumns(ICollection<ColumnDefinition> columnDefinitions, IDataReader sourceTableReader);
    }
}
