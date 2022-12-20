using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.AggregateHazardCharacterisationCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HazardCharacterisationCalculation.AggregateHazardCharacterisationCalculation {

    /// <summary>
    /// Tests for selecting/aggregating multiple hazard characterisations.
    /// </summary>
    [TestClass]
    public class AggregateHazardCharacterisationCalculatorTests {

        /// <summary>
        /// Simple test hazard characterisations aggregator.
        /// Given:
        /// Some mock substances, with one mock hazard characterisation
        /// per substance. Test whether the hazard characterisation will
        /// just pass these as the selected hazard characterisations.
        /// </summary>
        [TestMethod]
        public void AggregateHazardCharacterisationCalculator_TestSimple() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = MockEffectsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(3);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effects.First(), substances, seed: seed).Values;
            var calculator = new AggregateHazardCharacterisationCalculator();
            var result = calculator.SelectTargetDoses(
                substances,
                effects.First(),
                hazardCharacterisations,
                TargetDoseSelectionMethod.Aggregate,
                random
            );
            Assert.AreEqual(3, result.Count);
            var expectedValues = hazardCharacterisations.Select(r => r.Value).ToList();
            var resultValues = result.Values.Select(r => r.Value).ToList();
            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }
    }
}
