using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class MultipleExposureHazardRatioSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize (uncertainty), test MultipleExposureThresholdRatioSection view
        /// </summary>
        [TestMethod]
        public void MultipleExposureHazardRatioSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new MultipleExposureHazardRatioSection() { };
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var substances = MockSubstancesGenerator.Create(10);
            var individualEffectsDict = new Dictionary<Compound, List<IndividualEffect>>();
            var cumulativeExposureHazardRatio = new List<IndividualEffect>();

            foreach (var substance in substances) {
                var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
                individualEffectsDict[substance] = individualEffects;
            }

            for (int i = 0; i < 100; i++) {
                cumulativeExposureHazardRatio.Add(new IndividualEffect() {
                    SamplingWeight = individualEffectsDict[substances.First()].ElementAt(i).SamplingWeight,
                    CriticalEffectDose = individualEffectsDict[substances.First()].ElementAt(i).CriticalEffectDose,
                    ExposureConcentration = individualEffectsDict[substances.First()].ElementAt(i).CriticalEffectDose / individualEffectsDict[substances.First()].ElementAt(i).HazardExposureRatio,
                });
            }

            section.SummarizeMultipleSubstances(
                individualEffectsDict,
                cumulativeExposureHazardRatio,
                substances,
                new Effect() { Name = "effect" },
                RiskMetricCalculationType.RPFWeighted,
                RiskMetricType.HazardIndex,
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
                cumulativeExposureHazardRatio,
                false,
                2.5,
                97.5,
                true
            );

            Assert.AreEqual(11, section.RiskRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].RiskP50UncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PLowerRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PUpperRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PLowerRiskUncLower));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[1].PUpperRiskUncUpper));
            RenderView(section, filename: "MultipleHExposureHazardRatioSection_TestSummarize.html");
        }
    }
}
