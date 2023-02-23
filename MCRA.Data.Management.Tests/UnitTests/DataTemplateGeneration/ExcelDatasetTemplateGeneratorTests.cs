using MCRA.Data.Management.DataTemplateGeneration;
using MCRA.General;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataTemplateGeneration {

    [TestClass]
    public class ExcelDatasetTemplateGeneratorTests {
        private static string _outputBasePath = "ExcelDatasetTemplateGeneratorTests";

        [TestMethod]
        [DataRow(SourceTableGroup.Survey)]
        [DataRow(SourceTableGroup.Concentrations)]
        public void ExcelDatasetTemplateGenerator_TestCreate(SourceTableGroup tableGroup) {
            var outputFolder = TestUtilities.GetOrCreateTestOutputPath(_outputBasePath);
            var targetFile = Path.Combine(outputFolder, "ExcelDataSetTemplateGenerator_TestCreate.xlsx");
            var creator = new ExcelDatasetTemplateGenerator(targetFile);

            creator.Create(tableGroup);

            // Assert file exists
            Assert.IsTrue(File.Exists(targetFile));
        }
    }
}
