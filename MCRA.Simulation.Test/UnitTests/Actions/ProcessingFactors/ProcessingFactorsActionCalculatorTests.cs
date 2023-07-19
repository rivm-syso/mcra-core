using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ProcessingFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ProcessingFactors action
    /// </summary>
    [TestClass]
    public class ProcessingFactorsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the ProcessingFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void ProcessingFactorsActionCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var processingTypes = MockProcessingTypesGenerator.Create(3);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = MockSubstancesGenerator.Create(3);
            var processingFactors = MockProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);

            var dataManager = new MockCompiledDataManager(new CompiledData() {
                AllProcessingFactors = processingFactors.ToList(),
                AllProcessingTypes = processingTypes.ToDictionary(c => c.Code),
            });

            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData {
                AllCompounds = substances
            };

            var calculator = new ProcessingFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.ProcessingFactors);
            Assert.IsNotNull(data.ProcessingFactorModels);
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, reportFileName: "TestLoad");
        }

        /// <summary>
        /// Runs the ProcessingFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void ProcessingFactorsActionCalculator_TestUncertain1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var processingTypes = MockProcessingTypesGenerator.Create(2);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = MockSubstancesGenerator.Create(2);
            var processingFactors = MockProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);

            var dataManager = new MockCompiledDataManager(new CompiledData() {
                AllProcessingFactors = processingFactors.ToList(),
                AllProcessingTypes = processingTypes.ToDictionary(c => c.Code),
            });

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsProcessing = true;
            project.ConcentrationModelSettings.IsDistribution = true;

            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData {
                AllCompounds = substances
            };

            var calculator = new ProcessingFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadUncertain1");
            Assert.IsNotNull(data.ProcessingFactors);
            Assert.IsNotNull(data.ProcessingFactorModels);
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, reportFileName: "TestLoadUncertain1");
        }

        /// <summary>
        /// Runs the ProcessingFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void ProcessingFactorsActionCalculator_TestUncertain2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var processingTypes = MockProcessingTypesGenerator.Create(2);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = MockSubstancesGenerator.Create(2);
            var processingFactors = MockProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);

            var dataManager = new MockCompiledDataManager(new CompiledData() {
                AllProcessingFactors = processingFactors.ToList(),
                AllProcessingTypes = processingTypes.ToDictionary(c => c.Code),
            });

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsProcessing = true;
            project.ConcentrationModelSettings.IsDistribution = true;
            project.ConcentrationModelSettings.AllowHigherThanOne = true;

            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData {
                AllCompounds = substances
            };

            var calculator = new ProcessingFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadUncertain2");
            Assert.IsNotNull(data.ProcessingFactors);
            Assert.IsNotNull(data.ProcessingFactorModels);
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, reportFileName: "TestLoadUncertain2");
        }
    }
}