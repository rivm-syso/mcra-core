using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
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

            public ICompiledDataManager GetOutputCompiledDataManager(int idOutput) {
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
            var task = new TaskData() {
                ActionType = ActionType.DietaryExposures
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
