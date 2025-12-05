using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using System.Data;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    /// <summary>
    /// CompoundsDataBulkCopierTests
    /// </summary>
    [TestClass]
    public class DoseResponseDataBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// Test concentration data bulk copier. Copy relational data.
        /// </summary>
        [TestMethod]
        public void DoseResponseDataBulkCopierTests_TestBulkCopyTwoWayDataTables() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("DoseResponseData/TwoWayDataTables"))) {
                reader.Open();
                var bulkCopier = new DoseResponseDataBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;

                var experimentsTable = getRawDataSourceTable(RawDataSourceTableID.DoseResponseExperiments, tables);
                var experiments = experimentsTable
                    .Rows
                    .OfType<DataRow>()
                    .Select(r => r["idExperiment"])
                    .GroupBy(r => r)
                    .ToDictionary(r => r.Key, r => r.Count());
                Assert.HasCount(3, experiments);

                var dosesTable = getRawDataSourceTable(RawDataSourceTableID.DoseResponseExperimentDoses, tables);
                Assert.HasCount(31, dosesTable.Rows);
                var dosesExperiments = dosesTable
                    .Rows
                    .OfType<DataRow>()
                    .Select(r => r["idExperiment"])
                    .GroupBy(r => r)
                    .ToDictionary(r => r.Key, r => r.Count());
                Assert.HasCount(3, dosesExperiments);

                var measurementsTable = getRawDataSourceTable(RawDataSourceTableID.DoseResponseExperimentMeasurements, tables);
                Assert.HasCount(31, measurementsTable.Rows);
                var measurementsExperiments = measurementsTable
                    .Rows
                    .OfType<DataRow>()
                    .Select(r => r["idExperiment"])
                    .GroupBy(r => r)
                    .ToDictionary(r => r.Key, r => r.Count());
                Assert.HasCount(3, measurementsExperiments);
            }
        }
    }
}
