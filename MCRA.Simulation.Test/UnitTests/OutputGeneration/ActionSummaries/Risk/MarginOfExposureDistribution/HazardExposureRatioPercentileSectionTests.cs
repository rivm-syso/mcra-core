using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, CumulativeMarginOfExposure
    /// </summary>
    [TestClass]
    public class HazardExposureRatioPercentileSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize with uncertainty and test view rendering.
        /// </summary>
        [TestMethod]
        public void HazardExposureRatioPercentileSection_TestSummarizeInverseFalse() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new RiskRatioPercentileSection() { };
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var referenceDose = FakeHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                targetUnit
            );
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var individualEffects = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);
            section.Summarize(
                individualEffects: individualEffects,
                percentages: [80],
                referenceDose: referenceDose,
                targetUnit,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                RiskMetricType.HazardExposureRatio,
                isInverseDistribution: false,
                hcSubgroupDependent: false,
                hasHCSubgroups: false,
                skipPrivacySensitiveOutputs: false
            );

            for (int i = 0; i < 10; i++) {
                var individualEffectsClone = new List<IndividualEffect>();
                foreach (var item in individualEffects) {
                    var exposure = item.Exposure + LogNormalDistribution.Draw(random, 0, 1);
                    var ie = new IndividualEffect() {
                        SimulatedIndividualId = item.SimulatedIndividualId,
                        SamplingWeight = item.SamplingWeight,
                        Individual = item.Individual,
                        Exposure = exposure,
                        CriticalEffectDose = item.CriticalEffectDose,
                        EquivalentTestSystemDose = item.EquivalentTestSystemDose,
                        HazardExposureRatio = item.CriticalEffectDose / exposure,
                        ExposureHazardRatio = exposure / item.CriticalEffectDose
                    };
                    individualEffectsClone.Add(ie);
                }
                section.SummarizeUncertainty(individualEffectsClone, false, 2.5, 97.5);
            }
            var result = section.GetRiskPercentileRecords();
            Assert.AreEqual(result[0].ReferenceValue, section.Percentiles[0].ReferenceValue, 1e-3);
            Assert.IsTrue(result[0].ReferenceValue > 0);
            Assert.IsTrue(result[0].UpperBound > 0);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize with uncertainty and test view rendering.
        /// </summary>
        [TestMethod]
        public void HazardExposureRatioPercentileSection_TestSummarizeInverseTrue() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new RiskRatioPercentileSection() { };
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var referenceDose = FakeHazardCharacterisationModelsGenerator.CreateSingle(
               new Effect(),
               new Compound("Ref"),
               0.01,
               targetUnit
           );
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var individualEffects = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);
            section.Summarize(
                individualEffects,
                [80],
                referenceDose,
                targetUnit,
                RiskMetricCalculationType.RPFWeighted,
                RiskMetricType.HazardExposureRatio,
                true,
                hcSubgroupDependent: false,
                hasHCSubgroups: false,
                skipPrivacySensitiveOutputs: false
            );

            for (int i = 0; i < 10; i++) {
                var individualEffectsClone = new List<IndividualEffect>();
                foreach (var item in individualEffects) {
                    var exposure = item.Exposure + LogNormalDistribution.Draw(random, 0, 1);
                    var ie = new IndividualEffect() {
                        SimulatedIndividualId = item.SimulatedIndividualId,
                        SamplingWeight = item.SamplingWeight,
                        Individual = item.Individual,
                        Exposure = exposure,
                        CriticalEffectDose = item.CriticalEffectDose,
                        EquivalentTestSystemDose = item.EquivalentTestSystemDose,
                        HazardExposureRatio = item.CriticalEffectDose / exposure
                    };
                    individualEffectsClone.Add(ie);
                }
                section.SummarizeUncertainty(individualEffectsClone, false, 2.5, 97.5);
            }
            var result = section.GetRiskPercentileRecords();
            Assert.IsTrue(result[0].ReferenceValue > 0);
            Assert.IsTrue(result[0].LowerBound > 0);
            Assert.IsTrue(result[0].UpperBound > 0);
            AssertIsValidView(section);
        }
    }
}
