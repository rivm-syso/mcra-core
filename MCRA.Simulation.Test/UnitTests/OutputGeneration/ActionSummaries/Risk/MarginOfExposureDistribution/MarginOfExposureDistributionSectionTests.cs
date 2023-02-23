using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, CumulativeMarginOfExposure
    /// </summary>
    [TestClass]
    public class MarginOfExposureDistributionSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test CumulativeMarginOfExposureSection view
        /// </summary>
        [TestMethod]
        public void CumulativeMarginOfExposureSection_TestSummarize() {
            var referenceDose = MockHazardCharacterisationModelsGenerator.CreateSingle(
                new Effect(),
                new Compound("Ref"),
                0.01,
                TargetUnit.FromDoseUnit(DoseUnit.mgPerKgBWPerDay, null)
            );
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            var section = new MarginOfExposureDistributionSection();
            var percentageZero = individualEffects.Where(r => r.ExposureConcentration == 0).Count() / (double)individuals.Count * 100D;

            section.Summarize(90, 1, HealthEffectType.Risk, false, new double[] { 90, 95, 97.5, 99, 99.9 }, individualEffects, referenceDose);
            Assert.AreEqual(percentageZero, section.PercentageZeros);

            section.SummarizeUncertainty(individualEffects, false, 2.5, 97.5);
            AssertIsValidView(section);
        }
    }
}
