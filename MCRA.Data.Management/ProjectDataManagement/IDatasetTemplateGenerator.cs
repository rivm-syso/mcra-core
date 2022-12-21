using MCRA.General;

namespace MCRA.Data.Management {

    /// <summary>
    /// Interface for generator classes for creating datasets for tables
    /// of specific table groups.
    /// </summary>
    public interface IDatasetTemplateGenerator {

        /// <summary>
        /// Method to generate the template for the specified data source.
        /// </summary>
        /// <param name="sourceTableGroup">Source table group of the tables to create</param>
        /// <param name="dataFormatId">Optional data format id for a subset of the table group</param>
        void Create(SourceTableGroup sourceTableGroup, string dataFormatId = null);
    }
}
