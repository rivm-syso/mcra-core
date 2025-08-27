using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    /// <summary>
    /// CompoundsDataBulkCopierTests
    /// </summary>
    [TestClass]
    public class SurveyBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// Test concentration data bulk copier. Copy relational data.
        /// </summary>
        [TestMethod]
        public void SurveyBulkCopier_TestBulkCopyRelational() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("Consumptions/TestIndividualProperties"))) {
                reader.Open();
                var bulkCopier = new SurveyBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;

                var individualPropertiesTable = getRawDataSourceTable(RawDataSourceTableID.IndividualProperties, tables);
                Assert.AreEqual(5, individualPropertiesTable.Rows.Count);

                var propertyCodes = getDistinctColumnValues<string>(individualPropertiesTable, RawIndividualProperties.IdIndividualProperty.ToString()).ToArray();
                CollectionAssert.AreEquivalent(propertyCodes, new[] { "Age", "Gender", "Factor", "Salary", "bcode" });
            }
        }
    }
}
