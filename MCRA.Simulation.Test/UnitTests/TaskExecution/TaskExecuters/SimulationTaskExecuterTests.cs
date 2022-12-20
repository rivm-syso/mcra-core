using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.TaskExecution.TaskExecuters;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Simulation.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;

namespace MCRA.Simulation.Test.UnitTests.TaskExecution.TaskExecuters {
    /// <summary>
    /// Tests simulation task executer.
    /// </summary>
    [TestClass]
    public class SimulationTaskExecuterTests {

        protected static string _outputPath = Path.Combine(TestResourceUtilities.TestOutputPath, "TaskExecution");

        internal class MockTaskLoader : ITaskLoader {

            public (ProjectDto, ICompiledDataManager) Load(ITask task) {
                var effects = MockEffectsGenerator.Create(1);
                var substances = MockSubstancesGenerator.Create(3);
                var relativePotencyFactors = MockRelativePotencyFactorsGenerator.MockRelativePotencyFactors(substances);

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
                    ProjectDataSourceVersions = MockProjectDataSourcesGenerator
                        .FakeprojectDataSourceVersions(
                            0,
                            SourceTableGroup.Effects,
                            SourceTableGroup.Compounds,
                            SourceTableGroup.RelativePotencyFactors
                        )
                };
                project.EffectSettings.CodeFocalEffect = effects.First().Code;
                project.EffectSettings.CodeReferenceCompound = substances.First().Code;
                project.AssessmentSettings.MultipleSubstances = true;
                project.AssessmentSettings.Cumulative = true;
                return (project, dataManager);
            }

            public List<IOutput> CollectSubTaskOutputs(ITask task) {
                throw new NotImplementedException();
            }

            public ICompiledDataManager GetOutputCompiledDataManager(int idOutput) {
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
