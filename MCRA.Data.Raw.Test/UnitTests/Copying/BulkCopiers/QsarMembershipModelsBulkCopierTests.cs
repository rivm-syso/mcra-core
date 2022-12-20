using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    [TestClass]
    public class QsarMembershipModelsBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopySSD
        /// </summary>
        [TestMethod]
        public void QsarMembershipModelsBulkCopier_TestBulkCopyTabulatedXls() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new ExcelFileReader(TestUtils.GetResource("QsarMembershipModels/QsarMembershipModelsTabulated.xls"))) {
                reader.Open();
                var bulkCopier = new QsarMembershipModelsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                var modelsTableDefinition = getTableDefinition(RawDataSourceTableID.QsarMembershipModels);
                var membershipsTableDefinition = getTableDefinition(RawDataSourceTableID.QsarMembershipScores);

                var tables = dataSourceWriter.DataTables;

                Assert.AreEqual(2, tables[modelsTableDefinition.TargetDataTable].Rows.Count);
                Assert.AreEqual(8, tables[membershipsTableDefinition.TargetDataTable].Rows.Count);

                var modelIds = getDistinctColumnValues<string>(tables[modelsTableDefinition.TargetDataTable], RawQSARMembershipModels.Id.ToString()).ToArray();
                CollectionAssert.AreEquivalent(
                    new[] { "QSAR-M1", "QSAR-M2" },
                    modelIds
                );

                var modelScoreSubstanceIds = getDistinctColumnValues<string>(tables[membershipsTableDefinition.TargetDataTable], RawQSARMembershipScores.IdSubstance.ToString());
                CollectionAssert.AreEquivalent(
                    new[] { "SubstanceA", "SubstanceB", "SubstanceC", "SubstanceD" },
                    modelScoreSubstanceIds
                );
            }
        }

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopySSD
        /// </summary>
        [TestMethod]
        public void QsarMembershipModelsBulkCopier_TestBulkCopyTabulated() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("QsarMembershipModels/Tabulated"))) {
                reader.Open();
                var bulkCopier = new QsarMembershipModelsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                var modelsTableDefinition = getTableDefinition(RawDataSourceTableID.QsarMembershipModels);
                var membershipsTableDefinition = getTableDefinition(RawDataSourceTableID.QsarMembershipScores);

                var tables = dataSourceWriter.DataTables;

                Assert.AreEqual(2, tables[modelsTableDefinition.TargetDataTable].Rows.Count);
                Assert.AreEqual(9, tables[membershipsTableDefinition.TargetDataTable].Rows.Count);

                var modelIds = getDistinctColumnValues<string>(tables[modelsTableDefinition.TargetDataTable], RawQSARMembershipModels.Id.ToString()).ToArray();
                CollectionAssert.AreEquivalent(
                    new[] { "QSAR-M1", "QSAR-M2" },
                    modelIds
                );

                var modelScoreSubstanceIds = getDistinctColumnValues<string>(tables[membershipsTableDefinition.TargetDataTable], RawQSARMembershipScores.IdSubstance.ToString());
                CollectionAssert.AreEquivalent(
                    new[] { "SubstanceA", "SubstanceB", "SubstanceC", "SubstanceD", "SubstanceE" },
                    modelScoreSubstanceIds
                );
            }
        }
    }
}
