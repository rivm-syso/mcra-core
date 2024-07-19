using MCRA.Data.Management.DataTemplateGeneration;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataTemplateGeneration {

    [TestClass]
    public class ExcelDatasetTemplateGeneratorTests {
        private static string _outputBasePath = "ExcelDatasetTemplateGeneratorTests";

        [TestMethod]
        public void ExcelDatasetTemplateGenerator_TestCreateAll() {
            var outputFolder = TestUtilities.GetOrCreateTestOutputPath(_outputBasePath);
            var dataTableGroups = Enum.GetValues<SourceTableGroup>()
                .Where(McraModuleDefinitions.Instance.ModuleDefinitionsByTableGroup.ContainsKey);

            foreach (var tableGroup in dataTableGroups) {
                var targetFile = Path.Combine(outputFolder, $"ExcelTemplate_{tableGroup}.xlsx");
                var creator = new ExcelDatasetTemplateGenerator(targetFile);
                creator.Create(tableGroup);
                // Assert file exists
                Assert.IsTrue(File.Exists(targetFile));
            }
        }
    }
}
