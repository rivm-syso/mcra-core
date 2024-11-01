using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class HazardExposureRatioDistributionSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize and test CumulativeThresholdExposureRatioSection view.
        /// </summary>
        [TestMethod]
        public void CumulativeHazardExposureRatioSection_TestSummarize() {
            var targetUnit = TargetUnit.FromExternalDoseUnit(DoseUnit.mgPerKgBWPerDay, ExposureRoute.Oral);
            var referenceDose = MockHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                targetUnit
            );
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            var section = new RiskRatioDistributionSection();
            var percentageZero = individualEffects.Count(r => !r.IsPositive) / (double)individuals.Count * 100D;

            section.Summarize(
                confidenceInterval: 90,
                threshold: 1,
                isInverseDistribution: false,
                individualEffects: individualEffects,
                RiskMetricType.HazardExposureRatio
            );
            Assert.AreEqual(percentageZero, section.PercentageZeros);

            section.SummarizeUncertainty(individualEffects, false, 2.5, 97.5, RiskMetricType.HazardExposureRatio);
            AssertIsValidView(section);
        }
    }
}
