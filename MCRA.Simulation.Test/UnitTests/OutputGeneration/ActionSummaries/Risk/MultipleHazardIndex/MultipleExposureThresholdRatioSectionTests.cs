using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, MultipleExposureThresholdRatio
    /// </summary>
    [TestClass]
    public class MultipleExposureThresholdRatioSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize (uncertainty), test MultipleExposureThresholdRatioSection view
        /// </summary>
        [TestMethod]
        public void MultipleExposureThresholdRatioSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new MultipleExposureThresholdRatioSection() { };
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var substances = MockSubstancesGenerator.Create(10);
            var individualEffectsDict = new Dictionary<Compound, List<IndividualEffect>>();
            var cumulativeExposureThresholdRatio = new List<IndividualEffect>();

            foreach (var substance in substances) {
                var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
                individualEffectsDict[substance] = individualEffects;
            }

            for (int i = 0; i < 100; i++) {
                cumulativeExposureThresholdRatio.Add(new IndividualEffect() {
                    SamplingWeight = individualEffectsDict[substances.First()].ElementAt(i).SamplingWeight,
                    CriticalEffectDose = individualEffectsDict[substances.First()].ElementAt(i).CriticalEffectDose,
                    ExposureConcentration = individualEffectsDict[substances.First()].ElementAt(i).CriticalEffectDose / individualEffectsDict[substances.First()].ElementAt(i).ThresholdExposureRatio,
                });
            }

            section.SummarizeMultipleSubstances(
                individualEffectsDict,
                cumulativeExposureThresholdRatio,
                substances,
                new Effect() { Name = "effect" },
                RiskMetricCalculationType.RPFWeighted,
                90,
                1,
                HealthEffectType.Risk,
                5,
                10,
                false,
                false,
                true,
                onlyCumulativeOutput: false
            );
            section.SummarizeMultipleSubstancesUncertainty(
                substances,
                individualEffectsDict,
                cumulativeExposureThresholdRatio,
                RiskMetricCalculationType.RPFWeighted,
                false,
                2.5,
                97.5,
                true);

            Assert.AreEqual(11, section.ExposureThresholdRatioRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.ExposureThresholdRatioRecords[1].RiskP50UncP50));
            Assert.IsTrue(!double.IsNaN(section.ExposureThresholdRatioRecords[1].PLowerRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.ExposureThresholdRatioRecords[1].PUpperRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.ExposureThresholdRatioRecords[1].PLowerRisk_UncLower));
            Assert.IsTrue(!double.IsNaN(section.ExposureThresholdRatioRecords[1].PUpperRisk_UncUpper));
            RenderView(section, filename: "MultipleHExposureThresholdRatioSection_TestSummarize.html");
        }
    }
}
