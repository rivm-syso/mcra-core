using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.FoodSpecificSubstanceConversionCalculaton;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FoodSpecificSubstanceConversionCalculation {

    [TestClass]
    public class FoodSpecificSubstanceConversionCalculatorTests {

        [TestMethod]
        public void FoodSpecificSubstanceConversionCalculator_TestCompute() {
            var foods = FakeFoodsGenerator.Create(2);

            // Substances and substance conversions
            var activeSubstances = FakeSubstancesGenerator.Create(["AS1", "AS2"]);
            var measuredSubstances = FakeSubstancesGenerator.Create(["MS1", "MS2"]);

            // Create a sample substance collection (one sample and one measurement)
            var sampleSubstanceCollection = foods
                .Select(food => {
                    var sampleSubstanceRecord = fakeSampleSubstanceRecord(measuredSubstances, activeSubstances, [0.1, 0.1]);
                    return new SampleCompoundCollection(food, [sampleSubstanceRecord]);
                })
                .ToList();

            // Create fake conversion factor for first food, active substance, and measured substance
            var conversionFactors = new List<DeterministicSubstanceConversionFactor>();
            var factor = new DeterministicSubstanceConversionFactor() {
                MeasuredSubstance = measuredSubstances[0],
                ActiveSubstance = activeSubstances[0],
                Food = foods[0],
                ConversionFactor = 2
            };
            conversionFactors.Add(factor);

            // Act: compute
            var result = FoodSpecificSubstanceConversionCalculator
                .Compute(sampleSubstanceCollection, conversionFactors)
                .ToDictionary(r => r.Food);

            // Assert: check residues to all be 0.1, except for the cases for which we have a conversion factor
            for (int i = 0; i < foods.Count; i++) {
                for (int j = 0; j < measuredSubstances.Count; j++) {
                    for (int k = 0; k < activeSubstances.Count; k++) {
                        var sampleCompound = result[foods[i]].SampleCompoundRecords.First().SampleCompounds[activeSubstances[k]];
                        if (sampleCompound.MeasuredSubstance == measuredSubstances[j]) {
                            if (i == 0 && j == 0 && k == 0) {
                                Assert.AreEqual(0.2, sampleCompound.Residue);
                            } else {
                                Assert.AreEqual(0.1, sampleCompound.Residue);
                            }
                        }
                    }
                }
            }
        }

        protected static SampleCompoundRecord fakeSampleSubstanceRecord(
            List<Compound> measuredSubstances,
            List<Compound> activeSubstances,
            double[] concentrations
        ) {
            return new SampleCompoundRecord() {
                SampleCompounds = activeSubstances
                    .Select((r, ix) => {
                        var measured = measuredSubstances[ix % measuredSubstances.Count];
                        var missing = double.IsNaN(concentrations[ix]);
                        var resType = double.IsNaN(concentrations[ix]) ?
                            ResType.MV
                            : (concentrations[ix] >= 0) ? ResType.VAL : ResType.LOQ;
                        return new SampleCompound() {
                            MeasuredSubstance = measured,
                            ActiveSubstance = r,
                            Residue = resType == ResType.VAL ? concentrations[ix] : double.NaN,
                            ResType = resType,
                            Loq = resType == ResType.LOQ ? -concentrations[ix] : double.NaN,
                        };
                    })
                    .ToDictionary(r => r.ActiveSubstance)
            };
        }
    }
}
