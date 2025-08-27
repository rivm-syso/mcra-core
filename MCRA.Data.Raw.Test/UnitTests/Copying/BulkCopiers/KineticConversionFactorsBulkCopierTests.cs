using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    [TestClass]
    public class KineticConversionFactorsBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// KineticConversionFactorsBulkCopierTests
        /// </summary>
        [TestMethod]
        public void KineticConversionFactorsBulkCopierTests_TestXls() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new ExcelFileReader(TestUtils.GetResource("KineticConversionFactors/300124_Kinetic_conversion_factor_CAG_NAN_One_metabolite_Best_case_scenario_PP.xlsx"))) {
                reader.Open();
                var bulkCopier = new KineticConversionFactorsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                var KcfTableDefinition = getTableDefinition(RawDataSourceTableID.KineticConversionFactors);
                var SgTableDefinition = getTableDefinition(RawDataSourceTableID.KineticConversionFactorSGs);

                var tables = dataSourceWriter.DataTables;

                Assert.AreEqual(30, tables[KcfTableDefinition.TargetDataTable].Rows.Count);
                Assert.AreEqual(180, tables[SgTableDefinition.TargetDataTable].Rows.Count);

                var modelIds = getDistinctColumnValues<string>(tables[SgTableDefinition.TargetDataTable], RawKineticConversionFactorSGs.IdKineticConversionFactor.ToString());

            }
        }
    }
}
