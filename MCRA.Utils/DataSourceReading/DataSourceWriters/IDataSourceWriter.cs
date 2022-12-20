using MCRA.Utils.ProgressReporting;
using System;
using System.Collections.Generic;
using System.Data;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Interface of a data source writer that can be implemented for writing data
    /// to a specified target such as a file or a database.
    /// </summary>
    public interface IDataSourceWriter : IDisposable {

        /// <summary>
        /// Opens the reader for reading the data.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the data reader.
        /// </summary>
        void Close();

        /// <summary>
        /// Writes the data table to the specified destination table.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="tableDefinition"></param>
        void Write(DataTable data, string destinationTableName, TableDefinition tableDefinition);

        /// <summary>
        /// Writes the source table specified by the table definition to the specified destination table.
        /// </summary>
        /// <param name="sourceTableReader"></param>
        /// <param name="tableDefinition"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="progressState">Progress indicator, optional</param>
        void Write(IDataReader sourceTableReader, TableDefinition tableDefinition, string destinationTableName, ProgressState progressState = null);

    }
}
