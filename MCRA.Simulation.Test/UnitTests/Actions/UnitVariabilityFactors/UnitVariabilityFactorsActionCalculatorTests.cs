using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.UnitVariabilityFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the UnitVariabilityFactors action
    /// </summary>
    [TestClass]
    public class UnitVariabilityFactorsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the UnitVariabilityFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void UnitVariabilityFactorsActionCalculator_Test() {
            var foods = FakeFoodsGenerator.Create(3);
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var unitvariabilityFactors = FakeUnitVariabilityFactorsGenerator.Create(foods, substances, random);
            var compiledData = new CompiledData() {
                AllUnitVariabilityFactors = unitvariabilityFactors.SelectMany(c => c.Value.UnitVariabilityFactors).ToList(),
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                ModelledFoods = foods,
            };
            var calculator = new UnitVariabilityFactorsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.UnitVariabilityDictionary);
        }
    }
}
