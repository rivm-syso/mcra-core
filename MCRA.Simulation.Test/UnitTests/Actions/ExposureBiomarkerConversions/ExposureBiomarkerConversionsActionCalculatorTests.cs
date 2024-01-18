using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ExposureBiomarkerConversions;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ExposureBiomarkerConversions action
    /// </summary>
    [TestClass]
    public class ExposureBiomarkerConversionsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ExposureBiomarkerConversions action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void ExposureBiomarkerConversionsActionCalculator_TestLoadAndSummarize() {
            // Arrange
            var substances = MockSubstancesGenerator.Create(9);
            var compiledData = new CompiledData() {
                AllExposureBiomarkerConversions = MockExposureBiomarkerConversionsGenerator.Create(substances),

            };
            var project = new ProjectDto();
            var data = new ActionData() {
                AllCompounds = substances
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new ExposureBiomarkerConversionsActionCalculator(project);

            // Act
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, $"ExposureBiomarkerConversions");

            // Assert
            Assert.IsNotNull(data.ExposureBiomarkerConversions);
            Assert.AreEqual(4, data.ExposureBiomarkerConversions.Count);

            WriteReport(header, "TestLoadAndSummarize.html");
        }
    }
}
