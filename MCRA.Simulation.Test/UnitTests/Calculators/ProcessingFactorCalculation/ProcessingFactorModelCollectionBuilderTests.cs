using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {
    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class ProcessingFactorModelCollectionBuilderTests {
        /// <summary>
        /// Unit test not implemented
        /// </summary>
        [TestMethod]
        public void ProcessingFactorModelCollectionBuilder_TestCreateNone() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(3);
            var processingFactors = FakeProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);
            var settings = new ProcessingFactorsModuleConfig {
                IsProcessing = false,
                IsDistribution = false,
                AllowHigherThanOne = false
            };
            var builder = new ProcessingFactorModelCollectionBuilder(settings);
            var result = builder.Create(processingFactors, substances);
            Assert.AreEqual(0, result.Values.Count);
        }

        /// <summary>
        /// Unit test not implemented
        /// </summary>
        [TestMethod]
        public void ProcessingFactorModelCollectionBuilder_TestCreateFixed() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(3);
            var processingFactors = FakeProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);
            var settings = new ProcessingFactorsModuleConfig {
                IsProcessing = true,
                IsDistribution = false,
                AllowHigherThanOne = false
            };
            var builder = new ProcessingFactorModelCollectionBuilder(settings);
            var result = builder.Create(processingFactors, substances);
            Assert.AreEqual(processingFactors.Count, result.Values.Count);
        }

        /// <summary>
        /// Unit test not implemented
        /// </summary>
        [TestMethod]
        public void ProcessingFactorModelCollectionBuilder_TestCreateDistributionResample() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(1);
            var processingTypes = FakeProcessingTypesGenerator.Create(2);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(2);
            var processingFactors = FakeProcessingFactorsGenerator
                .Create(processedFoods, substances, random, processingTypes, true);
            var settings = new ProcessingFactorsModuleConfig {
                IsProcessing = true,
                IsDistribution = true,
                AllowHigherThanOne = false
            };
            var builder = new ProcessingFactorModelCollectionBuilder(settings);
            var result = builder.Create(processingFactors, substances);
            Assert.AreEqual(processingFactors.Count, result.Values.Count);
            Assert.IsTrue(result.Values.All(r => r is IDistributionProcessingFactorModel));

            builder.Resample(random, result);
            Assert.IsTrue(result.Values.All(r => r.IsUncertaintySample()));

            builder.ResetNominal(result);
            Assert.IsTrue(result.Values.All(r => !r.IsUncertaintySample()));
        }
    }
}
