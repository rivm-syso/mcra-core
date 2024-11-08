using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.NonDietaryExposureSources;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the NonDietaryExposureSources action
    /// </summary>
    [TestClass]
    public class NonDietaryExposureSourcesActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        ///  Runs the NonDietaryExposureSources action: load data and summarize method.
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureSourcesActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var sources = FakeNonDietaryExposureSourcesGenerator.Create(5);
            var compiledData = new CompiledData() {
                AllNonDietaryExposureSources = sources.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new NonDietaryExposureSourcesActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.NonDietaryExposureSources);
            Assert.AreEqual(5, data.NonDietaryExposureSources.Count);
        }
    }
}