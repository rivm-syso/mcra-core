using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.Populations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Tests the Populations action calculator.
    /// </summary>
    [TestClass]
    public class PopulationsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the Populations action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void PopulationsActionCalculator_Test() {
            var populations = new List<Population>();
            var propertyValueDict = new Dictionary<string, PopulationIndividualPropertyValue>();
            var propertyValue = new PopulationIndividualPropertyValue() {
                IndividualProperty = MockIndividualPropertiesGenerator.FakeAgeProperty,
                MinValue = 0,
                MaxValue = 10,
            };
            propertyValueDict["Age"] = propertyValue;
            var population = new Population() {
                Code = "NL",
                Description = "Description",
                Location = "Netherlands",
                Name = "Dutch",
                PopulationIndividualPropertyValues = propertyValueDict 
            };
            populations.Add(population);
            var compiledData = new CompiledData() {
                AllPopulations = populations.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData();
            var calculator = new PopulationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.SelectedPopulation);
        }
    }
}