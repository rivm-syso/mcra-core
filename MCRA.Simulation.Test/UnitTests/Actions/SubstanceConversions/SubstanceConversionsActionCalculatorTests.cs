using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.SubstanceConversions;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the SubstanceConversions action
    /// </summary>
    [TestClass]
    public class SubstanceConversionsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the SubstanceConversions action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void SubstanceConversionsActionCalculator_TestLoad() {
            var seed = 1;
            var substances = MockSubstancesGenerator.Create(12);
            var activeSubstances = substances.Skip(6).ToList();
            var measuredSubstances = substances.Take(6).ToList();
            var substanceConversions = MockSubstanceConversionsGenerator
                .Create(measuredSubstances, activeSubstances, seed);
            var compiledData = new CompiledData() {
                AllSubstanceConversions = substanceConversions.ToList(),
            };
            var data = new ActionData() {
                ActiveSubstances = substances,
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new SubstanceConversionsActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.SubstanceConversions);
        }
    }
}