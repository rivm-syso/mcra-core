using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Data;

namespace MCRA.Data.Raw.Copying {

    /// <summary>
    /// Copier class for copying raw data source data to the target
    /// database, based on the table definitions.
    /// </summary>
    public sealed class RawDataSourceBulkCopier {

        private readonly IDataSourceWriter _dataSourceWriter;

        /// <summary>
        /// Creates a new instance for the target database connection.
        /// </summary>
        /// <param name="dataSourceWriter">Connection to the target database</param>
        public RawDataSourceBulkCopier(IDataSourceWriter dataSourceWriter) {
            _dataSourceWriter = dataSourceWriter;
        }

        public List<SourceTableGroup> CopyFromDataTables(
            DataTable[] rawDataTables,
            bool allowEmptyDataSource = false,
            IRawDataSourceVersion rds = null,
            CompositeProgressState progressState = null,
            IEnumerable<SourceTableGroup> tableGroups = null
        ) {
            var dataSourceReader = new RawDataTableReader(rawDataTables);
            return copy(
                dataSourceReader,
                progressState ?? new CompositeProgressState(),
                rds,
                allowEmptyDataSource,
                tableGroups
            );
        }

        /// <summary>
        /// Copies the tables of the data file to the backend database.
        /// </summary>
        /// <param name="rawDataSourceFileName"></param>
        /// <param name="allowEmptyDataSource">Specifies whether to allow copying empty data sources or to throw an exception.</param>
        /// <param name="rawDataSourceFileName">Data source filename.</param>
        /// <param name="rds"></param>
        /// <param name="progressState">Progress state</param>
        public List<SourceTableGroup> CopyFromDataFile(
            string rawDataSourceFileName,
            bool allowEmptyDataSource = false,
            IRawDataSourceVersion rds = null,
            CompositeProgressState progressState = null,
            IEnumerable<SourceTableGroup> tableGroups = null
        ) {
            var dataSourceReader = DataSourceReaderFactory.GetDataReader(rawDataSourceFileName);
            return copy(
                dataSourceReader,
                progressState ?? new CompositeProgressState(),
                rds,
                allowEmptyDataSource,
                tableGroups
            );
        }

        /// <summary>
        /// Copies all tables of the data source reader to the backend database.
        /// </summary>
        /// <param name="dataSourceReader"></param>
        /// <param name="allowEmptyDataSource">Specifies whether to allow copying empty data sources or to throw an exception.</param>
        /// <param name="rds"></param>
        /// <param name="progressState">Progress state</param>
        /// <param name="tableGroups">Optional: can be used to restrict copying to just the specified table groups.</param>
        public List<SourceTableGroup> CopyFromDataSourceReader(
            IDataSourceReader dataSourceReader,
            bool allowEmptyDataSource = false,
            CompositeProgressState progressState = null,
            IRawDataSourceVersion rds = null,
            IEnumerable<SourceTableGroup> tableGroups = null
        ) {
            return copy(
                dataSourceReader,
                progressState ?? new CompositeProgressState(),
                rds,
                allowEmptyDataSource,
                tableGroups
            );
        }

        /// <summary>
        /// Copies all tables of the <see cref="IDataSourceReader"/> to the data source writer
        /// (e.g., a writer that writes the data to the backend database).
        /// </summary>
        /// <param name="dataSourceReader"></param>
        /// <param name="progressState">Progress state</param>
        /// <param name="rds"></param>
        /// <param name="allowEmptyDataSource">Specifies whether to allow copying empty data sources or to throw an exception.</param>
        /// <param name="tableGroups">Optional: can be used to restrict copying to just the specified table groups.</param>
        /// <returns></returns>
        private List<SourceTableGroup> copy(
            IDataSourceReader dataSourceReader,
            CompositeProgressState progressState,
            IRawDataSourceVersion rds,
            bool allowEmptyDataSource,
            IEnumerable<SourceTableGroup> tableGroups = null
        ) {
            if (dataSourceReader is null) {
                throw new ArgumentNullException(nameof(dataSourceReader));
            }
            try {
                dataSourceReader.Open();
                if (tableGroups == null) {
                    tableGroups = Enum.GetValues(typeof(SourceTableGroup)).Cast<SourceTableGroup>();
                }
                var localProgress = progressState.NewProgressState(1D);
                localProgress.Update("Starting bulk copy...");
                var progressStepSize = 99D / tableGroups.Count();
                var parsedTableGroups = new HashSet<SourceTableGroup>();
                var parsedDataTables = new HashSet<RawDataSourceTableID>();
                var copiers = RawDataSourceBulkCopierFactory.Create(
                    dataSourceReader,
                    _dataSourceWriter,
                    parsedTableGroups,
                    parsedDataTables,
                    tableGroups
                );
                foreach (var copier in copiers) {
                    var copyProgress = progressState.NewProgressState(progressStepSize);
                    var tgs = copier.Copy(dataSourceReader, copyProgress);
                    parsedTableGroups.UnionWith(tgs);
                    copyProgress.Update(100);
                }
                if (!allowEmptyDataSource && !parsedTableGroups.Any()) {
                    throw new RawDataSourceBulkCopyException("The data source does not contain any recognized data type.");
                }
                foreach (var tableGroup in parsedTableGroups) {
                    rds?.RegisterTableGroup(tableGroup);
                }
                localProgress.Update("Finished", 100);
                return parsedTableGroups.ToList();
            } finally {
                dataSourceReader.Close();
            }
        }
    }
}
