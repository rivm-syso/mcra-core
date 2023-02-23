using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    /// <summary>
    /// PopulationsDataBulkCopierTests
    /// </summary>
    [TestClass]
    public class PopulationsBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// Test concentration data bulk copier. Copy relational data.
        /// </summary>
        [TestMethod]
        public void PopulationsBulkCopier_TestBulkCopyRelational() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("Populations/TestPopulationProperties"))) {
                reader.Open();
                var bulkCopier = new PopulationsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());
                var tables = dataSourceWriter.DataTables;

                var individualPropertiesTable = getRawDataSourceTable(RawDataSourceTableID.IndividualProperties, tables);
                Assert.AreEqual(6, individualPropertiesTable.Rows.Count);

                var propertyCodes = getDistinctColumnValues<string>(individualPropertiesTable, RawIndividualProperties.IdIndividualProperty.ToString()).ToArray();
                CollectionAssert.AreEquivalent(propertyCodes, new[] { "Age", "Gender", "Region", "Month", "Period", "Height"});

                var individualPropertyValuesTable = getRawDataSourceTable(RawDataSourceTableID.PopulationIndividualPropertyValues, tables);

                var generalPopRows = individualPropertyValuesTable.Select("idPopulation = 'AB-Pop-2001'");
                var generalPopProperties = generalPopRows.Select(r => r["idIndividualProperty"]).ToList();
                Assert.IsTrue(!generalPopProperties.Any());

                var childPopRows = individualPropertyValuesTable.Select("idPopulation = 'AB-Children-2001'");
                var childPopProperties = childPopRows.Select(r => r["idIndividualProperty"]).ToList();
                CollectionAssert.AreEquivalent(childPopProperties, new[] { "Age", "Period" });

                Assert.AreEqual(6, individualPropertiesTable.Rows.Count);

            }
        }
    }
}
