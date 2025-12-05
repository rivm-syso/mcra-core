using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardDistribution
    /// </summary>
    [TestClass]
    public class HazardPercentileSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize and test HazardPercentileSection view
        /// </summary>
        [TestMethod]
        public void HazardPercentileSection_TestSummarize1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var targetUnit = TargetUnit.FromExternalDoseUnit(DoseUnit.mgPerKgBWPerDay, ExposureRoute.Oral);
            var referenceDose = FakeHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                targetUnit
            );
            var individuals = FakeIndividualsGenerator.CreateSimulated(100, 1, random);
            var individualEffects = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var section = new HazardPercentileSection();
            section.Summarize(individualEffects, [95], referenceDose);

            for (int i = 0; i < 10; i++) {
                var uncertainHazard = individualEffects
                    .Select(c => new IndividualEffect() {
                        CriticalEffectDose = c.CriticalEffectDose + LogNormalDistribution.Draw(random, 0, 1),
                        SimulatedIndividual = c.SimulatedIndividual,
                    })
                    .ToList();
                section.SummarizeUncertainty(uncertainHazard, 2.5, 97.5);
            }
            var result = section.GetHazardPercentileRecords();
            Assert.AreEqual(result[0].ReferenceValue, section.Percentiles[0].ReferenceValue);
            Assert.IsGreaterThan(0, result[0].ReferenceValue);
            Assert.IsGreaterThan(0, result[0].LowerBound);
            Assert.IsGreaterThan(0, result[0].UpperBound);

            var percentiles = new UncertainDataPointCollection<double>() {
                XValues = new List<double>() { 50, 95 },
                ReferenceValues = new List<double>() { 1.24, 3.6 },
            };
            percentiles.AddUncertaintyValues(new List<double>() { 1.23, 7 });
            section.Percentiles = percentiles;
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize and test HazardPercentileSection view
        /// </summary>
        [TestMethod]
        public void HazardPercentileSection_TestSummarize2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var targetUnit = TargetUnit.FromExternalDoseUnit(DoseUnit.mgPerKgBWPerDay, ExposureRoute.Oral);
            var referenceDose = FakeHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                targetUnit
            );
            var individuals = FakeIndividualsGenerator.CreateSimulated(100, 1, random);
            var individualEffects = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var section = new HazardPercentileSection();
            section.Summarize(individualEffects, [95], referenceDose);

            for (int i = 0; i < 10; i++) {
                var uncertainHazard = individualEffects
                    .Select(c => new IndividualEffect() {
                        CriticalEffectDose = c.CriticalEffectDose + LogNormalDistribution.Draw(random, 0, 1),
                        SimulatedIndividual = c.SimulatedIndividual
                    })
                    .ToList();
                section.SummarizeUncertainty(uncertainHazard, 2.5, 97.5);
            }
            var result = section.GetHazardPercentileRecords();

            Assert.AreEqual(result[0].ReferenceValue, section.Percentiles[0].ReferenceValue);
            Assert.IsGreaterThan(0, result[0].ReferenceValue);
            Assert.IsGreaterThan(0, result[0].LowerBound);
            Assert.IsGreaterThan(0, result[0].UpperBound);
            AssertIsValidView(section);
        }
    }
}
