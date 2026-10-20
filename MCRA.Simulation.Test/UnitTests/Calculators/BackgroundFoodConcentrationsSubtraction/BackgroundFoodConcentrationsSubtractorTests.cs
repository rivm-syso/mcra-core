using MCRA.General;
using MCRA.Simulation.Calculators.BackgroundFoodConcentrationsSubtraction;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.BackgroundFoodConcentrationsSubtraction {

    [TestClass]
    public class BackgroundFoodConcentrationsSubtractorTests {

        [TestMethod]
        public void BackgroundFoodConcentrationsSubtractor_TestSubtract() {
            var seed = 1;
            var foods = FakeFoodsGenerator.Create(4);
            var substances = FakeSubstancesGenerator.Create(4);
            var allFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var sampleCompoundCollection = SampleCompoundCollectionsBuilder
                .Create(foods, substances, allFoodSamples, ConcentrationUnit.mgPerKg, null);
            var backgroundConcentrations = FakeBackgroundFoodConcentrationsGenerator
                .Create(foods.Take(2), substances.Take(2), seed: seed);

            var result = BackgroundFoodConcentrationsSubtractor.Subtract(
                sampleCompoundCollection, backgroundConcentrations, ConcentrationUnit.mgPerKg
            );
            foreach (var record in backgroundConcentrations) {
                var sumAfter = result[record.Food].SampleCompoundRecords.Sum(r => r.SampleCompounds[record.Substance].IsPositiveResidue ? r.SampleCompounds[record.Substance].Residue : 0D);
                var expectedAfter = sampleCompoundCollection[record.Food].SampleCompoundRecords.Sum(r => r.SampleCompounds[record.Substance].IsPositiveResidue ? Math.Max(r.SampleCompounds[record.Substance].Residue - record.Value, 0) : 0D);
                Assert.AreEqual(expectedAfter, sumAfter, 1e-4);
            }
        }
    }
}
