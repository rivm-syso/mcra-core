using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    [TestClass]
    public class ProcessingFactorModelCollectionBuilderTests {

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        public void ProcessingFactorModelCollectionBuilder_TestCreateFixed(
            bool isDistribution,
            bool allowHigherThanOne
        ) {
            var random = new McraRandomGenerator(1);
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(3);
            var processingFactors = FakeProcessingFactorsGenerator
                .Create(processedFoods, substances, random, processingTypes);
            var builder = new ProcessingFactorModelCollectionBuilder();
            var result = builder.Create(processingFactors, isDistribution, allowHigherThanOne);
            Assert.HasCount(processingFactors.Count, result);
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        public void ProcessingFactorModelCollectionBuilder_TestCreateSubstanceGeneric(
            bool isDistribution,
            bool allowHigherThanOne
        ) {
            var random = new McraRandomGenerator(1);
            var foods = FakeFoodsGenerator.Create(3);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(3);
            substances.Add(null); // Add a null substance to test substance-generic processing factors
            var processingFactors = FakeProcessingFactorsGenerator
                .Create(processedFoods, substances, random, processingTypes);
            var builder = new ProcessingFactorModelCollectionBuilder();
            var result = builder.Create(processingFactors, isDistribution, allowHigherThanOne);
            Assert.HasCount(processingFactors.Count, result);
        }

        [TestMethod]
        public void ProcessingFactorModelCollectionBuilder_TestCreateDistribution() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(1);
            var processingTypes = FakeProcessingTypesGenerator.Create(2);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(2);
            var processingFactors = FakeProcessingFactorsGenerator
                .Create(processedFoods, substances, random, processingTypes, true);
            var builder = new ProcessingFactorModelCollectionBuilder();
            var result = builder.Create(processingFactors, true, false);
            Assert.HasCount(processingFactors.Count, result);
            Assert.IsTrue(result.All(r => r is IDistributionProcessingFactorModel));
        }

        [TestMethod]
        public void ProcessingFactorModelCollectionBuilder_TestResample() {
            var random = new McraRandomGenerator(1);
            var foods = FakeFoodsGenerator.Create(1);
            var processingTypes = FakeProcessingTypesGenerator.Create(2);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(foods, processingTypes);
            foods.AddRange(processedFoods);
            var substances = FakeSubstancesGenerator.Create(2);
            var processingFactors = FakeProcessingFactorsGenerator
                .Create(processedFoods, substances, random, processingTypes, true);
            var builder = new ProcessingFactorModelCollectionBuilder();
            var result = builder.Create(processingFactors, true, false);

            builder.Resample(random, result);
            Assert.IsTrue(result.All(r => r.IsUncertaintySample()));

            builder.ResetNominal(result);
            Assert.IsTrue(result.All(r => !r.IsUncertaintySample()));
        }
    }
}
