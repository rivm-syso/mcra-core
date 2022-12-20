using MCRA.Data.Raw.Copying.EuHbmDataCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers;
using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.EuHbmDataCopiers {
    [TestClass]
    public class EuHbmImportDataCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// Tests copying of EU HBM import.
        /// </summary>
        [TestMethod]
        public void EuHbmImportDataCopier_TestCopy() {
            var testFile = "HumanMonitoring/EU-HBM-Import-Artificial.xlsx";
            var parsedTables = new HashSet<RawDataSourceTableID>();
            var parsedTableGroups = new HashSet<SourceTableGroup>();
            using (var dataSourceWriter = new DataTableDataSourceWriter()) {
                using (var reader = new ExcelFileReader(TestUtils.GetResource(testFile))) {
                    reader.Open();
                    var bulkCopier = new EuHbmImportDataCopier(dataSourceWriter, parsedTableGroups, parsedTables);
                    bulkCopier.TryCopy(reader, new ProgressState());
                    var tables = dataSourceWriter.DataTables;
                    void checkTableRecordsCount(RawDataSourceTableID tableId, int count) {
                        var tableDefinition = getTableDefinition(tableId);
                        Assert.AreEqual(count, tables[tableDefinition.TargetDataTable].Rows.Count);
                    }
                    checkTableRecordsCount(RawDataSourceTableID.HumanMonitoringSurveys, 1);
                    checkTableRecordsCount(RawDataSourceTableID.Individuals, 10);
                    checkTableRecordsCount(RawDataSourceTableID.IndividualProperties, 3);
                    checkTableRecordsCount(RawDataSourceTableID.IndividualPropertyValues, 30);
                    checkTableRecordsCount(RawDataSourceTableID.Compounds, 9);
                    checkTableRecordsCount(RawDataSourceTableID.AnalyticalMethods, 2);
                    checkTableRecordsCount(RawDataSourceTableID.AnalyticalMethodCompounds, 9);
                    checkTableRecordsCount(RawDataSourceTableID.HumanMonitoringSamples, 40);
                    checkTableRecordsCount(RawDataSourceTableID.HumanMonitoringSampleAnalyses, 40);
                    checkTableRecordsCount(RawDataSourceTableID.HumanMonitoringSampleConcentrations, 180);
                }
            }
            Assert.IsTrue(parsedTableGroups.Contains(SourceTableGroup.Compounds));
            Assert.IsTrue(parsedTableGroups.Contains(SourceTableGroup.HumanMonitoringData));
            Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.HumanMonitoringSampleAnalyses));
            Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.HumanMonitoringSampleConcentrations));
            Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.HumanMonitoringSamples));
            Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.HumanMonitoringSurveys));
            Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.AnalyticalMethods));
            Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.AnalyticalMethodCompounds));
        }
    }
}
