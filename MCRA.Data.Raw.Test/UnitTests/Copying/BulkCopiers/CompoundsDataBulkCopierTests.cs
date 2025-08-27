using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Converters;
using MCRA.Data.Raw.Copying.BulkCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.General;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    /// <summary>
    /// CompoundsDataBulkCopierTests
    /// </summary>
    [TestClass]
    public class CompoundsDataBulkCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopySSD
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopyCompoundsFromSsd() {
            var dataSourceWriter = new DataTableDataSourceWriter();
            using (var reader = new ExcelFileReader(TestUtils.GetResource("Concentrations/ConcentrationsSSD.xls"))) {
                reader.Open();
                var bulkCopier = new CompoundsBulkCopier(dataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                var tables = dataSourceWriter.DataTables;
                var tableDefinition = getTableDefinition(RawDataSourceTableID.Compounds);

                Assert.AreEqual(4, tables[tableDefinition.TargetDataTable].Rows.Count);

                var substanceIds = getDistinctColumnValues<string>(
                    tables[tableDefinition.TargetDataTable],
                    RawCompounds.IdCompound.ToString()
                );
                CollectionAssert.AreEqual(new[] { "CompoundA", "CompoundB", "CompoundC", "CompoundD" }, substanceIds);
            }
        }

        /// <summary>
        /// ConcentrationDataBulkCopier_TestBulkCopySSD
        /// </summary>
        [TestMethod]
        public void ConcentrationDataBulkCopier_TestBulkCopyCompoundsFromSsdWithRecoding() {
            var substanceCodeConversions = new EntityCodeConversionsCollection() {
                IdEntity = "Compounds",
                ConversionTuples = [
                    new EntityCodeConversionTuple("CompoundA", "SubstanceA"),
                    new EntityCodeConversionTuple("CompoundX", "SubstanceX")
                ]
            };
            var dataSourceWriter = new DataTableDataSourceWriter();
            var recodingDataSourceWriter = new RecodingDataSourceWriter(
                dataSourceWriter,
                substanceCodeConversions
            );
            using (var reader = new ExcelFileReader(TestUtils.GetResource("Concentrations/ConcentrationsSSD.xls"))) {
                reader.Open();
                var bulkCopier = new CompoundsBulkCopier(recodingDataSourceWriter, null, null);
                bulkCopier.TryCopy(reader, new ProgressState());

                var tables = dataSourceWriter.DataTables;
                var tableDefinition = getTableDefinition(RawDataSourceTableID.Compounds);

                Assert.AreEqual(4, tables[tableDefinition.TargetDataTable].Rows.Count);

                var substanceIds = getDistinctColumnValues<string>(
                    tables[tableDefinition.TargetDataTable],
                    RawCompounds.IdCompound.ToString()
                );
                CollectionAssert.AreEqual(new[] { "SubstanceA", "CompoundB", "CompoundC", "CompoundD" }, substanceIds);
            }
        }
    }
}
