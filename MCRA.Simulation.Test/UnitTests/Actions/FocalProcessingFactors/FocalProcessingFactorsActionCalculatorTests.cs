using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ProcessingFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the FocalProcessingFactors action
    /// </summary>
    [TestClass]
    public class FocalProcessingFactorsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the ProcessingFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void FocalProcessingFactorsActionCalculator_ReplaceProcFactTest() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(3);
            var processingFactors = FakeProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);
            var focalProcessingFactors = FakeProcessingFactorsGenerator.Create(
                FakeFoodsGenerator.CreateProcessedFoods([foods[0]], [processingTypes[0]]),
                [substances[0]],
                random,
                [processingTypes[0]]
            );
            var dataManager = new MockCompiledDataManager(new CompiledData() {
                AllProcessingFactors = processingFactors.ToList(),
                AllProcessingTypes = processingTypes.ToDictionary(c => c.Code),
            });

            var project = new ProjectDto();
            var config = project.ProcessingFactorsSettings;
            config.UseFocalProcessingFactors = true;
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData {
                AllCompounds = substances,
                FocalProcessingFactors = [.. focalProcessingFactors],
            };

            var calculator = new ProcessingFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.ProcessingFactors);
            Assert.IsNotNull(data.ProcessingFactorModels);
            Assert.HasCount(27, data.ProcessingFactorModels);
        }

        /// <summary>
        /// Runs the ProcessingFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void FocalProcessingFactorsActionCalculator_AddProcFactTest() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(4);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods([.. foods.Take(3)], processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(3);
            var processingFactors = FakeProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);
            
            var focalProcessingFactors = FakeProcessingFactorsGenerator.Create(
                FakeFoodsGenerator.CreateProcessedFoods([foods[4]], [processingTypes[0]]),
                [substances[0]],
                random,
                [processingTypes[0]]
            );
            var dataManager = new MockCompiledDataManager(new CompiledData() {
                AllProcessingFactors = [.. processingFactors],
                AllProcessingTypes = processingTypes.ToDictionary(c => c.Code),
            });

            var project = new ProjectDto();
            var config = project.ProcessingFactorsSettings;
            config.UseFocalProcessingFactors = true;
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData {
                AllCompounds = substances,
                FocalProcessingFactors = focalProcessingFactors.ToList(),
            };

            var calculator = new ProcessingFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.ProcessingFactors);
            Assert.IsNotNull(data.ProcessingFactorModels);
            Assert.HasCount(28, data.ProcessingFactorModels);
        }
    }
}