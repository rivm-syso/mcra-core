using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.TaskExecution.TaskExecuters;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.TaskExecution.TaskExecuters {

    /// <summary>
    /// Loop task executer tests.
    /// </summary>
    [TestClass]
    public class LoopCalculationTaskExecuterTests {

        protected static string _outputPath = Path.Combine(TestUtilities.TestOutputPath, "TaskExecution");

        internal class MockLoopTaskLoader : ITaskLoader {

            public int NumberOfSubtasks { get; set; } = 4;

            public (ProjectDto, ICompiledDataManager) Load(ITask task) {
                throw new NotImplementedException();
            }

            public List<IOutput> CollectSubTaskOutputs(ITask task) {
                var result = Enumerable
                    .Range(0, NumberOfSubtasks)
                    .Select(r => new OutputData {
                        id = r,
                        Description = $"Subtask {NumberOfSubtasks + 3 * r}"
                    })
                    .Cast<IOutput>()
                    .ToList();
                return result;
            }
           
            /// <summary>
            /// Returns the compiled data managers for the raw data, per (sub)action, that was generated for specified output.
            /// </summary>
            /// <param name="idOutput">Identifier of an action output.</param>
            public Dictionary<ActionType?, ICompiledDataManager> GetOutputCompiledDataManagers(int idOutput) {
                var result = new Dictionary<ActionType?, ICompiledDataManager>() {
                    { ActionType.Risks, GetOutputCompiledDataManagerRisks(idOutput) },
                    { ActionType.DietaryExposures, GetOutputCompiledDataManagerDietary(idOutput) },
                };
                return result;
            }

            private ICompiledDataManager GetOutputCompiledDataManagerRisks(int idOutput) {
                var models = MockRiskModelsGenerator.CreateMockRiskModels(
                    new[] { "$Risk output {idOutput}" },
                    new[] { 50, 90, 95, 97.5, 99, 99.9, 99.99 },
                    -1,
                    1
                );
                var compiledData = new CompiledData() {
                    AllRiskModels = models.ToDictionary(r => r.Code),
                };
                return new MockCompiledDataManager(compiledData);
            }

            private ICompiledDataManager GetOutputCompiledDataManagerDietary(int idOutput) {
                var random = new McraRandomGenerator(idOutput);
                var models = MockDietaryExposureModelsGenerator.CreateMockDietaryExposureModels(
                    new[] { "$Exposures output {idOutput}" },
                    new[] { 50, 90, 95, 97.5, 99, 99.9, 99.99 },
                    -1,
                    random
                );
                var compiledData = new CompiledData() {
                    AllDietaryExposureModels = models.ToDictionary(r => r.Code),
                };
                return new MockCompiledDataManager(compiledData);
            }
        }

        /// <summary>
        /// Test successful run of loop calculation task executer.
        /// </summary>
        [TestMethod]
        public void LoopCalculationTaskExecuter_TestSuccess() {
            var project = new ProjectDto();
            var xmlString = ProjectSettingsSerializer.ExportToXmlString(project);
            var task = new TaskData() {
                ActionType = ActionType.Risks,
                SettingsXml = xmlString
            };
            var outputManager = new StoreLocalOutputManager(Path.Combine(_outputPath, "LoopCalculationTaskExecuter_TestSuccess")) {
                WriteReport = true
            };
            var taskLoader = new MockLoopTaskLoader();

            var simulationExecuter = new LoopCalculationTaskExecuter(taskLoader, outputManager, string.Empty);
            simulationExecuter.Run(task, new ProgressReport());
        }
    }
}
