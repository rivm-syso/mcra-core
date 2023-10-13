using MCRA.Utils.Statistics;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var foods = MockFoodsGenerator.Create(3);
            var processingTypes = MockProcessingTypesGenerator.Create(3);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = MockSubstancesGenerator.Create(3);
            var processingFactors = MockProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);
            var settings = new ProcessingFactorModelCollectionBuilderSettings(new ConcentrationModelSettings() {
                IsProcessing = false,
                IsDistribution = false,
                AllowHigherThanOne = false
            });
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
            var foods = MockFoodsGenerator.Create(3);
            var processingTypes = MockProcessingTypesGenerator.Create(3);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = MockSubstancesGenerator.Create(3);
            var processingFactors = MockProcessingFactorsGenerator.Create(processedFoods, substances, random, processingTypes);
            var settings = new ProcessingFactorModelCollectionBuilderSettings(new ConcentrationModelSettings() {
                IsProcessing = true,
                IsDistribution = false,
                AllowHigherThanOne = false
            });
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
            var foods = MockFoodsGenerator.Create(1);
            var processingTypes = MockProcessingTypesGenerator.Create(2);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = MockSubstancesGenerator.Create(2);
            var processingFactors = MockProcessingFactorsGenerator
                .Create(processedFoods, substances, random, processingTypes, true);
            var settings = new ProcessingFactorModelCollectionBuilderSettings(new ConcentrationModelSettings() {
                IsProcessing = true,
                IsDistribution = true,
                AllowHigherThanOne = false
            });
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
