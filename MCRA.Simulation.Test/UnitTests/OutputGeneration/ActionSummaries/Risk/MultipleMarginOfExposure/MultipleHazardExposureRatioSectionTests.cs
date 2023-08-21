using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, MultipleThresholdExposureRatio
    /// </summary>
    [TestClass]
    public class MultipleHazardExposureRatioSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize (uncertainty), test MultipleThresholdExposureRatioSection view
        /// </summary>
        [TestMethod]
        public void MultipleHazardExposureRatioSection_TestSummarize() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var substances = MockSubstancesGenerator.Create(10);
            var individualEffectsBySubstance = new Dictionary<Compound, List<IndividualEffect>>();
            var individualEffects = new List<IndividualEffect>();

            foreach (var substance in substances) {
                individualEffectsBySubstance[substance] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }

            for (int i = 0; i < 100; i++) {
                individualEffects.Add(new IndividualEffect() {
                    SamplingWeight = individualEffectsBySubstance[substances.First()].ElementAt(i).SamplingWeight,
                    CriticalEffectDose = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose,
                    ExposureConcentration = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose / individualEffectsBySubstance[substances.First()].ElementAt(i).HazardExposureRatio,
                    HazardExposureRatio = individualEffectsBySubstance[substances.First()].ElementAt(i).HazardExposureRatio
                });
            }

            var section = new MultipleHazardExposureRatioSection() { };
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance,
                individualEffects,
                substances,
                null,
                1,
                90,
                HealthEffectType.Risk,
                RiskMetricType.MarginOfExposure,
                RiskMetricCalculationType.RPFWeighted,
                5,
                10,
                false,
                true,
                onlyCumulativeOutput: false
            );
            section.SummarizeMultipleSubstancesUncertainty(
                substances,
                individualEffectsBySubstance,
                individualEffects,
                RiskMetricCalculationType.RPFWeighted,
                false,
                2.5,
                97.5,
                true);

            Assert.AreEqual(11, section.RiskRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].RiskP50UncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PLowerRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PUpperRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PLowerRiskUncLower));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PUpperRiskUncUpper));
            AssertIsValidView(section);
        }
    }
}
