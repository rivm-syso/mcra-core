using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.DoseResponseData;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var testSystems = FakeTestSystemsGenerator.Create(3);
            var responses = FakeResponsesGenerator.Create(
                testSystems,
                responsesPerTestSystem: 2,
                responseTypes: [ResponseType.ContinuousMultiplicative, ResponseType.Quantal]
            );
            var substances = FakeSubstancesGenerator.Create(3);

            var doseResponseExperiments = FakeDoseResponseExperimentsGenerator
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
            Assert.HasCount(doseResponseExperiments.Count, data.AvailableDoseResponseExperiments);
            Assert.HasCount(doseResponseExperiments.Count, data.SelectedResponseExperiments);
            writeOutput(data, "TestLoadNonMixtures");
        }

        /// <summary>
        /// Runs the DoseResponseData action.
        /// </summary>
        [TestMethod]
        public void DoseResponseDataActionCalculator_TestLoadMixtureData() {
            var substances = FakeSubstancesGenerator.Create(3);
            var responses = FakeResponsesGenerator.Create(2);
            var doseResponseExperiments = FakeDoseResponseExperimentsGenerator.Create(substances, responses, true);

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
            Assert.HasCount(doseResponseExperiments.Count, data.AvailableDoseResponseExperiments);
            Assert.HasCount(doseResponseExperiments.Count, data.SelectedResponseExperiments);
            WriteReport(header, "TestLoadMixtureData.html");
            writeOutput(data, "TestLoadMixtureData");
        }

        /// <summary>
        /// Runs the DoseResponseData action:
        /// MergeDoseResponseExperimentsData = true;
        /// </summary>
        [TestMethod]
        public void DoseResponseDataActionCalculator_TestMergeExperiments() {
            var substances = FakeSubstancesGenerator.Create(3);
            var responses = FakeResponsesGenerator.Create(2);
            var doseResponseExperiments = FakeDoseResponseExperimentsGenerator.Create(substances, responses);

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
            Assert.HasCount(doseResponseExperiments.Count, data.AvailableDoseResponseExperiments);
            Assert.HasCount(responses.Count, data.SelectedResponseExperiments);
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
