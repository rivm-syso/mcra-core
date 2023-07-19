using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.Responses;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the Responses action
    /// </summary>
    [TestClass]
    public class ResponsesActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the Responses action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void ResponsesActionCalculator_TestLoad() {
            var testSystems = MockTestSystemsGenerator.Create(4);
            var responses = MockResponsesGenerator.Create(testSystems, 10);

            var compiledData = new CompiledData() {
                AllTestSystems = testSystems.ToDictionary(r => r.Code),
                AllResponses = responses.ToDictionary(c => c.Code),
            };

            var data = new ActionData();
            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new ResponsesActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.Responses);
        }
    }
}