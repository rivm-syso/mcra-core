﻿using MCRA.Data.Raw.Copying.EuHbmDataCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers;
using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.EuHbmDataCopiers {
    [TestClass]
    public class EuHbmImportDataCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// Tests copying of EU HBM import.
        /// </summary>
        [TestMethod]
        [DataRow("EU-HBM-Import-Artificial_v2.0.xlsx")]
        [DataRow("EU-HBM-Import-Artificial_v2.1.xlsx")]
        [DataRow("EU-HBM-Import-Artificial_v2.2.xlsx")]
        public void EuHbmImportDataCopier_TestCopy(string formatVersion) {
            var testFile = $"HumanMonitoring/{formatVersion}";
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
                    checkTableRecordsCount(RawDataSourceTableID.IndividualProperties, 5);
                    checkTableRecordsCount(RawDataSourceTableID.IndividualPropertyValues, 50);
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

        /// <summary>
        /// Tests version of EU HBM import. Throw exception.
        /// </summary>
        [TestMethod]
        [DataRow("EU-HBM-Import-Artificial_v12.81.xlsx")]
        public void EuHbmImportDataCopier_TestVersion(string formatVersion) {
            var testFile = $"HumanMonitoring/{formatVersion}";
            var parsedTables = new HashSet<RawDataSourceTableID>();
            var parsedTableGroups = new HashSet<SourceTableGroup>();
            using (var dataSourceWriter = new DataTableDataSourceWriter()) {
                using (var reader = new ExcelFileReader(TestUtils.GetResource(testFile))) {
                    reader.Open();
                    var bulkCopier = new EuHbmImportDataCopier(dataSourceWriter, parsedTableGroups, parsedTables);
                    Assert.ThrowsException<Exception>(() => bulkCopier.TryCopy(reader, new ProgressState()));
                }
            }
        }

        /// <summary>
        /// Empty col header in one of the sample time point sheets should be ignored and not caused any exceptions
        /// and/or result in abort of import.
        /// See column C in sheet SAMPLETIMEPOINT_US of the file under test, which has an empty col header.
        /// </summary>
        [TestMethod]
        [DataRow("EU-HBM-Import-Artificial_v2.2-empty-col-header.xlsx")]
        public void EuHbmImportDataCopier_EmptyColHeaderSubstances_ShouldIgnoreEmptyHeader(string file) {
            var testFile = $"HumanMonitoring/{file}";
            var parsedTables = new HashSet<RawDataSourceTableID>();
            var parsedTableGroups = new HashSet<SourceTableGroup>();
            using (var dataSourceWriter = new DataTableDataSourceWriter()) {
                using (var reader = new ExcelFileReader(TestUtils.GetResource(testFile))) {
                    reader.Open();
                    var bulkCopier = new EuHbmImportDataCopier(dataSourceWriter, parsedTableGroups, parsedTables);
                    bulkCopier.TryCopy(reader, new ProgressState());
                }

                var tableDefinition = getTableDefinition(RawDataSourceTableID.Compounds);
                Assert.AreEqual(9, dataSourceWriter.DataTables[tableDefinition.TargetDataTable].Rows.Count);
                Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.Compounds));
            }
        }
    }
}
