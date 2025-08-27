using MCRA.Data.Raw.Copying.PbkModelUploadCopiers;
using MCRA.Data.Raw.Test.Helpers;
using MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers;
using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.PbkModelUploadCopiers {
    [TestClass]
    public class PbkUploadCopierTests : BulkCopierTestsBase {

        /// <summary>
        /// Tests copying of PBK model definitions import.
        /// </summary>
        [TestMethod]
        [DataRow("EuroMixGenericPbk.sbml")]
        public void PbkUploadCopier_TestCopy(string filename) {
            var outputPath = TestUtils.CreateTestOutputPath("PbkUploadCopier_TestCopy");

            var testFile = Path.Combine("PbkModels", filename);
            var sbmlFilePath = TestUtils.GetResource(testFile);
            var parsedTables = new HashSet<RawDataSourceTableID>();
            var parsedTableGroups = new HashSet<SourceTableGroup>();

            using (var dataSourceWriter = new CsvDataSourceWriter(new DirectoryInfo(outputPath))) {
                using (var reader = new SbmlDataSourceReader(sbmlFilePath)) {
                    var bulkCopier = new PbkModelUploadCopier(dataSourceWriter, parsedTableGroups, parsedTables);
                    bulkCopier.TryCopy(reader, new ProgressState());
                }
            }
            Assert.IsTrue(parsedTableGroups.Contains(SourceTableGroup.PbkModelDefinitions));
            Assert.IsTrue(parsedTables.Contains(RawDataSourceTableID.KineticModelDefinitions));
            var fileSbml = Path.Combine(outputPath, filename);
            var fileCsv = Path.Combine(outputPath, "RawPbkModelDefinitions.csv");
            Assert.IsTrue(File.Exists(fileSbml));
            Assert.IsTrue(File.Exists(fileCsv));
        }
    }
}
