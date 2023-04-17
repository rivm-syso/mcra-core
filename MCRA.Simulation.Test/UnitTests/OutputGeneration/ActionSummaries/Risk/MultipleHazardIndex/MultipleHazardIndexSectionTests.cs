using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, MultipleHazardIndex
    /// </summary>
    [TestClass]
    public class MultipleHazardIndexSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize (uncertainty), test MultipleHazardIndexSection view
        /// </summary>
        [TestMethod]
        public void MultipleHazardIndexSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new MultipleHazardIndexSection() { };
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var substances = MockSubstancesGenerator.Create(10);
            var individualEffectsDict = new Dictionary<Compound, List<IndividualEffect>>();
            var cumulativeHazardIndex = new List<IndividualEffect>();

            foreach (var substance in substances) {
                var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
                individualEffectsDict[substance] = individualEffects;
            }

            for (int i = 0; i < 100; i++) {
                cumulativeHazardIndex.Add(new IndividualEffect() {
                    SamplingWeight = individualEffectsDict[substances.First()].ElementAt(i).SamplingWeight,
                    CriticalEffectDose = individualEffectsDict[substances.First()].ElementAt(i).CriticalEffectDose,
                    ExposureConcentration = individualEffectsDict[substances.First()].ElementAt(i).CriticalEffectDose / individualEffectsDict[substances.First()].ElementAt(i).MarginOfExposure,
                });
            }

            section.SummarizeMultipleSubstances(
                individualEffectsDict,
                cumulativeHazardIndex,
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
                cumulativeHazardIndex,
                RiskMetricCalculationType.RPFWeighted,
                false,
                2.5,
                97.5,
                true);

            Assert.AreEqual(11, section.HazardIndexRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.HazardIndexRecords[1].HIP50UncP50));
            Assert.IsTrue(!double.IsNaN(section.HazardIndexRecords[1].PLowerHIUncP50));
            Assert.IsTrue(!double.IsNaN(section.HazardIndexRecords[1].PUpperHIUncP50));
            Assert.IsTrue(!double.IsNaN(section.HazardIndexRecords[1].PLowerHI_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardIndexRecords[1].PUpperHI_UncUpper));
            RenderView(section, filename: "MultipleHazardIndexSection_TestSummarize.html");
        }
    }
}
