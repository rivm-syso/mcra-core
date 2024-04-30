using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.DoseResponseData;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the DoseResponseData action.
    /// </summary>
    [TestClass]
    public class DoseResponseDataActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the DoseResponseData action.
        /// </summary>
        [TestMethod]
        public void DoseResponseDataActionCalculator_TestLoadNonMixtures() {
            var testSystems = MockTestSystemsGenerator.Create(3);
            var responses = MockResponsesGenerator.Create(
                testSystems,
                responsesPerTestSystem: 2,
                responseTypes: new ResponseType[] { ResponseType.ContinuousMultiplicative, ResponseType.Quantal }
            );
            var substances = MockSubstancesGenerator.Create(3);

            var doseResponseExperiments = MockDoseResponseExperimentsGenerator
                .Create(substances, responses, true);

            var compiledData = new CompiledData() {
                AllDoseResponseExperiments = doseResponseExperiments.ToDictionary(c => c.Code),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var data = new ActionData {
                Responses = responses.ToDictionary(c => c.Code)
            };
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new DoseResponseDataActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadNonMixtures");
            Assert.AreEqual(doseResponseExperiments.Count, data.AvailableDoseResponseExperiments.Count);
            Assert.AreEqual(doseResponseExperiments.Count, data.SelectedResponseExperiments.Count);
            writeOutput(data, "TestLoadNonMixtures");
        }

        /// <summary>
        /// Runs the DoseResponseData action.
        /// </summary>
        [TestMethod]
        public void DoseResponseDataActionCalculator_TestLoadMixtureData() {
            var substances = MockSubstancesGenerator.Create(3);
            var responses = MockResponsesGenerator.Create(2);
            var doseResponseExperiments = MockDoseResponseExperimentsGenerator.Create(substances, responses, true);

            var compiledData = new CompiledData() {
                AllDoseResponseExperiments = doseResponseExperiments.ToDictionary(c => c.Code),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var data = new ActionData {
                Responses = responses.ToDictionary(c => c.Code)
            };
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new DoseResponseDataActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadMixtureData");
            Assert.AreEqual(doseResponseExperiments.Count, data.AvailableDoseResponseExperiments.Count);
            Assert.AreEqual(doseResponseExperiments.Count, data.SelectedResponseExperiments.Count);
            WriteReport(header, "TestLoadMixtureData.html");
            writeOutput(data, "TestLoadMixtureData");
        }

        /// <summary>
        /// Runs the DoseResponseData action: 
        /// MergeDoseResponseExperimentsData = true;
        /// </summary>
        [TestMethod]
        public void DoseResponseDataActionCalculator_TestMergeExperiments() {
            var substances = MockSubstancesGenerator.Create(3);
            var responses = MockResponsesGenerator.Create(2);
            var doseResponseExperiments = MockDoseResponseExperimentsGenerator.Create(substances, responses);

            var compiledData = new CompiledData() {
                AllDoseResponseExperiments = doseResponseExperiments.ToDictionary(c => c.Code),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var config = new DoseResponseDataModuleConfig { MergeDoseResponseExperimentsData = true };
            var project = new ProjectDto(config);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData {
                Responses = responses.ToDictionary(c => c.Code)
            };
            var calculator = new DoseResponseDataActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad2");
            Assert.AreEqual(doseResponseExperiments.Count, data.AvailableDoseResponseExperiments.Count);
            Assert.AreEqual(responses.Count, data.SelectedResponseExperiments.Count);
            WriteReport(header, "TestMergeExperiments.html");
            writeOutput(data, "TestMergeExperiments");
        }

        private void writeOutput(ActionData data, string testId) {
            var outputPath = Path.Combine(_reportOutputPath, GetType().Name);
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
            foreach (var experiment in data.SelectedResponseExperiments) {
                var dt = experiment.CreateAllResponsesDataTable();
                var filename = $"{testId}-{experiment.Code}.csv";
                dt.ToCsv(Path.Combine(outputPath, filename));
            };
        }
    }
}
