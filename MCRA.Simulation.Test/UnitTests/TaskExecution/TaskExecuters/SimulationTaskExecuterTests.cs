using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.TaskExecution.TaskExecuters;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.Test.Mocks;
using MCRA.Utils.ProgressReporting;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.TaskExecution.TaskExecuters {
    /// <summary>
    /// Tests simulation task executer.
    /// </summary>
    [TestClass]
    public class SimulationTaskExecuterTests {

        protected static string _outputPath = Path.Combine(TestUtilities.TestOutputPath, "TaskExecution");

        internal class MockTaskLoader : ITaskLoader {

            public (ProjectDto, ICompiledDataManager) Load(ITask task) {
                var effects = FakeEffectsGenerator.Create(1);
                var substances = FakeSubstancesGenerator.Create(3);
                var relativePotencyFactors = FakeRelativePotencyFactorsGenerator.Create(substances, substances.First());

                var compiledData = new CompiledData() {
                    AllRelativePotencyFactors = new Dictionary<string, List<RelativePotencyFactor>>() {
                        { effects.First().Code, relativePotencyFactors }
                    },
                    AllEffects = effects.ToDictionary(c => c.Code),
                    AllSubstances = substances.ToDictionary(r => r.Code),
                };
                var dataManager = new MockCompiledDataManager(compiledData);

                var project = new ProjectDto() {
                    ActionType = ActionType.RelativePotencyFactors,
                    ProjectDataSourceVersions = FakeProjectDataSourcesGenerator
                        .FakeprojectDataSourceVersions(
                            0,
                            SourceTableGroup.Effects,
                            SourceTableGroup.Compounds,
                            SourceTableGroup.RelativePotencyFactors
                        )
                };
                project.ActiveSubstancesSettings.IsCompute = false;
                project.EffectsSettings.CodeFocalEffect = effects.First().Code;
                project.SubstancesSettings.CodeReferenceSubstance = substances.First().Code;
                project.SubstancesSettings.MultipleSubstances = true;
                project.ConcentrationModelsSettings.Cumulative = true;
                return (project, dataManager);
            }

            public List<IOutput> CollectSubTaskOutputs(ITask task) {
                throw new NotImplementedException();
            }

            public Dictionary<ActionType?, ICompiledDataManager> GetOutputCompiledDataManagers(int idOutput) {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Test successful run of simulation task executer.
        /// </summary>
        [TestMethod]
        public void SimulationTaskExecuter_TestSuccess() {
            var task = new TaskData();
            var mockTaskLoader = new MockTaskLoader();

            var outputManager = new StoreLocalOutputManager(Path.Combine(_outputPath, "SimulationTaskExecuter_TestSuccess")) {
                WriteReport = true
            };
            var simulationExecuter = new SimulationTaskExecuter(mockTaskLoader, outputManager, string.Empty) {
                KeepTemporaryFiles = true,
            };
            simulationExecuter.Run(task, new ProgressReport());
        }
    }
}
