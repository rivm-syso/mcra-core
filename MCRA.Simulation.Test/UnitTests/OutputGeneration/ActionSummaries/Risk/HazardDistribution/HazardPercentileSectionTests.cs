using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var referenceDose = MockHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                targetUnit
            );
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var section = new HazardPercentileSection();
            section.Summarize(individualEffects, new double[] { 95 }, referenceDose);

            for (int i = 0; i < 10; i++) {
                var uncertainHazard = individualEffects
                    .Select(c => new IndividualEffect() {
                        CriticalEffectDose = c.CriticalEffectDose + LogNormalDistribution.Draw(random, 0, 1),
                        SamplingWeight = c.SamplingWeight,
                    })
                    .ToList();
                section.SummarizeUncertainty(uncertainHazard, 2.5, 97.5);
            }
            var result = section.GetHazardPercentileRecords();
            Assert.AreEqual(result[0].ReferenceValue, section.Percentiles[0].ReferenceValue);
            Assert.IsTrue(result[0].ReferenceValue > 0);
            Assert.IsTrue(result[0].LowerBound > 0);
            Assert.IsTrue(result[0].UpperBound > 0);

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
            var referenceDose = MockHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                targetUnit
            );
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var section = new HazardPercentileSection();
            section.Summarize(individualEffects, new double[] { 95 }, referenceDose);

            for (int i = 0; i < 10; i++) {
                var uncertainHazard = individualEffects
                    .Select(c => new IndividualEffect() {
                        CriticalEffectDose = c.CriticalEffectDose + LogNormalDistribution.Draw(random, 0, 1),
                        SamplingWeight = c.SamplingWeight,
                    })
                    .ToList();
                section.SummarizeUncertainty(uncertainHazard, 2.5, 97.5);
            }
            var result = section.GetHazardPercentileRecords();

            Assert.AreEqual(result[0].ReferenceValue, section.Percentiles[0].ReferenceValue);
            Assert.IsTrue(result[0].ReferenceValue > 0);
            Assert.IsTrue(result[0].LowerBound > 0);
            Assert.IsTrue(result[0].UpperBound > 0);
            AssertIsValidView(section);
        }
    }
}
