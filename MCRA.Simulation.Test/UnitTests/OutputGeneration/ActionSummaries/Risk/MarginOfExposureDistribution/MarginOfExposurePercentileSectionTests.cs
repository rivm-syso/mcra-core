using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, CumulativeMarginOfExposure
    /// </summary>
    [TestClass]
    public class MarginOfExposurePercentileSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize with uncertainty and test MOEPercentileSection view
        /// </summary>
        [TestMethod]
        public void MoePercentileSection_TestSummarizeInverseFalse() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new MarginOfExposurePercentileSection() { };
            var referenceDose = MockHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                TargetUnit.FromDoseUnit(DoseUnit.mgPerKgBWPerDay)
            );
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            section.Summarize(individualEffects: individualEffects,
                percentages: new List<double>() { 80 },
                referenceDose: referenceDose,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                isInverseDistribution: false);

            for (int i = 0; i < 10; i++) {
                var individualEffectsClone = new List<IndividualEffect>();
                foreach (var item in individualEffects) {
                    var exposure = item.ExposureConcentration + LogNormalDistribution.Draw(random, 0, 1);
                    var ie = new IndividualEffect() {
                        CompartmentWeight = item.CompartmentWeight,
                        ExposureConcentration = exposure,
                        SamplingWeight = item.SamplingWeight,
                        SimulatedIndividualId = item.SimulatedIndividualId,
                        CriticalEffectDose = item.CriticalEffectDose,
                        EquivalentTestSystemDose = item.EquivalentTestSystemDose,
                        MarginOfExposure = item.CriticalEffectDose / exposure,
                        HazardIndex = exposure/item.CriticalEffectDose
                    };
                    individualEffectsClone.Add(ie);
                }
                section.SummarizeUncertainty(individualEffectsClone, false, 2.5, 97.5);
            }
            var result = section.GetMOEPercentileRecords();
            Assert.AreEqual(result[0].ReferenceValue, section.Percentiles[0].ReferenceValue);
            Assert.AreEqual(6.764, result[0].ReferenceValue, 1e-3);
            Assert.AreEqual(1.717, result[0].UpperBound, 1e-3);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize with uncertainty and test MOEPercentileSection view
        /// </summary>
        [TestMethod]
        public void MoePercentileSection_TestSummarizeInverseTrue() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new MarginOfExposurePercentileSection() { };
            var referenceDose = MockHazardCharacterisationModelsGenerator.CreateSingle(
               new Effect(),
               new Compound("Ref"),
               0.01,
               TargetUnit.FromDoseUnit(DoseUnit.mgPerKgBWPerDay)
           );
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            section.Summarize(individualEffects, new List<double>() { 80 }, referenceDose, HealthEffectType.Risk, RiskMetricCalculationType.RPFWeighted, true);

            for (int i = 0; i < 10; i++) {
                var individualEffectsClone = new List<IndividualEffect>();
                foreach (var item in individualEffects) {
                    var exposure = item.ExposureConcentration + LogNormalDistribution.Draw(random, 0, 1);
                    var ie = new IndividualEffect() {
                        CompartmentWeight = item.CompartmentWeight,
                        ExposureConcentration = exposure,
                        SamplingWeight = item.SamplingWeight,
                        SimulatedIndividualId = item.SimulatedIndividualId,
                        CriticalEffectDose = item.CriticalEffectDose,
                        EquivalentTestSystemDose = item.EquivalentTestSystemDose,
                        MarginOfExposure = item.CriticalEffectDose / exposure
                    };
                    individualEffectsClone.Add(ie);
                }
                section.SummarizeUncertainty(individualEffectsClone, false, 2.5, 97.5);
            }
            var result = section.GetMOEPercentileRecords();
            Assert.AreEqual(8.157, result[0].ReferenceValue, 1e-3);
            Assert.AreEqual(1.295, result[0].LowerBound, 1e-3);
            Assert.AreEqual(1.717, result[0].UpperBound, 1e-3);
            AssertIsValidView(section);
        }
    }
}
