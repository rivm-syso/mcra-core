using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, CumulativeThresholdExposureRatio
    /// </summary>
    [TestClass]
    public class ThresholdExposureRatioDistributionSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test CumulativeThresholdExposureRatioSection view
        /// </summary>
        [TestMethod]
        public void CumulativeThresholdExposureRatioSection_TestSummarize() {
            var referenceDose = MockHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                TargetUnit.FromDoseUnit(DoseUnit.mgPerKgBWPerDay)
            );
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            var section = new ThresholdExposureRatioDistributionSection();
            var percentageZero = individualEffects.Count(r => !r.IsPositive) / (double)individuals.Count * 100D;

            section.Summarize(
                confidenceInterval: 90,
                threshold: 1,
                healthEffectType: HealthEffectType.Risk,
                isInverseDistribution: false,
                selectedPercentiles: new double[] { 90, 95, 97.5, 99, 99.9 },
                individualEffects: individualEffects,
                referenceDose: referenceDose,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted);
            Assert.AreEqual(percentageZero, section.PercentageZeros);

            section.SummarizeUncertainty(individualEffects, false, 2.5, 97.5);
            AssertIsValidView(section);
        }
    }
}
