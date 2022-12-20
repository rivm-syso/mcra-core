using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    [TestClass]
    public class MolecularDockingModelsBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopySSD
        /// </summary>
        [TestMethod]
        public void MolecularDockingModelsBulkCopierTests_TestBulkCopyTabulatedXls() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new ExcelFileReader(TestUtils.GetResource("MolecularDockingModels/MolecularDockingModelsTabulated.xls"))) {
                reader.Open();
                var bulkCopier = new MolecularDockingModelsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                var modelsTableDefinition = getTableDefinition(RawDataSourceTableID.MolecularDockingModels);
                var membershipsTableDefinition = getTableDefinition(RawDataSourceTableID.MolecularBindingEnergies);

                var tables = dataSourceWriter.DataTables;

                Assert.AreEqual(2, tables[modelsTableDefinition.TargetDataTable].Rows.Count);
                Assert.AreEqual(8, tables[membershipsTableDefinition.TargetDataTable].Rows.Count);

                var modelIds = getDistinctColumnValues<string>(tables[modelsTableDefinition.TargetDataTable], RawMolecularDockingModels.Id.ToString()).ToArray();
                CollectionAssert.AreEquivalent(
                    new[] { "Dock-M1", "Dock-M2" },
                    modelIds
                );

                var modelScoreSubstanceIds = getDistinctColumnValues<string>(tables[membershipsTableDefinition.TargetDataTable], RawMolecularBindingEnergies.IdCompound.ToString());
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
        public void MolecularDockingModelsBulkCopierTests_TestBulkCopyTabulated() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new CsvFolderReader(TestUtils.GetResource("MolecularDockingModels/Tabulated"))) {
                reader.Open();
                var bulkCopier = new MolecularDockingModelsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                var modelsTableDefinition = getTableDefinition(RawDataSourceTableID.MolecularDockingModels);
                var membershipsTableDefinition = getTableDefinition(RawDataSourceTableID.MolecularBindingEnergies);

                var tables = dataSourceWriter.DataTables;

                Assert.AreEqual(2, tables[modelsTableDefinition.TargetDataTable].Rows.Count);
                Assert.AreEqual(9, tables[membershipsTableDefinition.TargetDataTable].Rows.Count);

                var modelIds = getDistinctColumnValues<string>(tables[modelsTableDefinition.TargetDataTable], RawMolecularDockingModels.Id.ToString()).ToArray();
                CollectionAssert.AreEquivalent(
                    new[] { "Dock-M1", "Dock-M2" },
                    modelIds
                );

                var modelScoreSubstanceIds = getDistinctColumnValues<string>(tables[membershipsTableDefinition.TargetDataTable], RawMolecularBindingEnergies.IdCompound.ToString());
                CollectionAssert.AreEquivalent(
                    new[] { "SubstanceA", "SubstanceB", "SubstanceC", "SubstanceD", "SubstanceE" },
                    modelScoreSubstanceIds
                );
            }
        }
    }
}
