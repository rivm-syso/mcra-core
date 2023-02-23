using MCRA.Data.Management.DataTemplateGeneration;
using MCRA.General;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataTemplateGeneration {

    [TestClass]
    public class ErdFileTemplateGeneratorTests {
        private static string _outputBasePath = "ErdFileGeneratorTests";

        [TestMethod]
        [DataRow(SourceTableGroup.Survey)]
        [DataRow(SourceTableGroup.Concentrations)]
        [DataRow(SourceTableGroup.HumanMonitoringData)]
        public void ErdFileGenerator_TestCreate(SourceTableGroup tableGroup) {
            var outputFolder = TestUtilities.GetOrCreateTestOutputPath(_outputBasePath);
            var targetFile = Path.Combine(outputFolder, $"ErdFileGenerator_TestCreate_{tableGroup}.er");
            var creator = new ErdFileGenerator(targetFile);

            creator.Create(tableGroup);

            // Assert file exists
            Assert.IsTrue(File.Exists(targetFile));
        }
    }
}
