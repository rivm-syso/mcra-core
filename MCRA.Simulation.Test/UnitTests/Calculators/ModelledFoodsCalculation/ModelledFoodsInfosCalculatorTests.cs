using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ModelledFoodsCalculation {
    /// <summary>
    /// FoodsAsMeasuredCalculation calculator
    /// </summary>
    [TestClass]
    public class ModelledFoodsInfosCalculatorTests {
        /// <summary>
        /// Calculate statistics for sample compound collections
        /// </summary>
        [TestMethod]
        public void SubstanceSampleStatisticsCalculation_Test1() {
            var foodA = new Food("FoodA");
            var substanceA = new Compound("SubstanceA");
            var foods = new List<Food>() { foodA };
            var substances = new List<Compound>() { substanceA };
            var sampleCompoundCollections = foods
                .Select(food => mockSampleCompoundCollection(food, substances))
                .ToDictionary(r => r.Food);

            var settings = new ModelledFoodsInfosCalculatorSettings(new() {
                DeriveModelledFoodsFromSampleBasedConcentrations = true,
                DeriveModelledFoodsFromSingleValueConcentrations = false,
                UseWorstCaseValues = false,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
            });
            var calculator = new ModelledFoodsInfosCalculator(settings);
            var result = calculator.Compute(foods, substances, sampleCompoundCollections, null, null)
                .Single();
            Assert.AreEqual(foodA, result.Food);
            Assert.AreEqual(substanceA, result.Substance);
            Assert.IsTrue(result.HasMeasurements);
            Assert.IsTrue(result.HasPositiveMeasurements);
        }

        private SampleCompoundCollection mockSampleCompoundCollection(Food food, ICollection<Compound> substances) {
            var sampleCompoundRecords = new List<SampleCompoundRecord>();
            for (int i = 0; i < 100; i++) {
                var sampleCompoundRecord = new SampleCompoundRecord() {
                    SampleCompounds = []
                };
                foreach (var substance in substances) {
                    var resType = i < 50 ? ResType.LOQ : ResType.VAL;
                    var sampleCompound = new SampleCompound(
                        compound: substance,
                        resType: resType,
                        residue: 0.5,
                        lod: 0.1,
                        loq: 0.1
                    );
                    sampleCompoundRecord.SampleCompounds.Add(substance, sampleCompound);
                }
                sampleCompoundRecords.Add(sampleCompoundRecord);
            }
            return new SampleCompoundCollection(food, sampleCompoundRecords);
        }
    }
}
