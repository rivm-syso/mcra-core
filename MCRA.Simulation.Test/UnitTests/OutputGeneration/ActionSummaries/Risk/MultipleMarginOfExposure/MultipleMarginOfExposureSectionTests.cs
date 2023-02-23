using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, MultipleMarginOfExposure
    /// </summary>
    [TestClass]
    public class MultipleMarginOfExposureSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize (uncertainty), test MultipleMarginOfExposureSection view
        /// </summary>
        [TestMethod]
        public void MultipleMarginOfExposureSection_TestSummarize() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new MultipleMarginOfExposureSection() { };
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var substances = MockSubstancesGenerator.Create(10);
            var individualEffectsBySubstance = new Dictionary<Compound, List<IndividualEffect>>();
            var cumulativeIndividualEffects = new List<IndividualEffect>();

            foreach (var substance in substances) {
                individualEffectsBySubstance[substance] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }

            for (int i = 0; i < 100; i++) {
                cumulativeIndividualEffects.Add(new IndividualEffect() {
                    SamplingWeight = individualEffectsBySubstance[substances.First()].ElementAt(i).SamplingWeight,
                    CriticalEffectDose = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose,
                    ExposureConcentration = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose / individualEffectsBySubstance[substances.First()].ElementAt(i).MarginOfExposure(HealthEffectType.Risk),
                });
            }

            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance, 
                cumulativeIndividualEffects, 
                substances, 
                null,
                1, 
                90, 
                HealthEffectType.Risk, 
                5, 
                10, 
                false, 
                true,
                onlyCumulativeOutput: false
            );
            section.SummarizeMultipleSubstancesUncertainty(
                substances,
                individualEffectsBySubstance,
                cumulativeIndividualEffects,
                false,
                2.5,
                97.5,
                true);

            Assert.AreEqual(11, section.MOERecords.Count);
            Assert.IsTrue(!double.IsNaN(section.MOERecords[1].MOEP50UncP50));
            Assert.IsTrue(!double.IsNaN(section.MOERecords[1].PLowerMOEUncP50));
            Assert.IsTrue(!double.IsNaN(section.MOERecords[1].PUpperMOEUncP50));
            Assert.IsTrue(!double.IsNaN(section.MOERecords[1].PLowerMOE_UncLower));
            Assert.IsTrue(!double.IsNaN(section.MOERecords[1].PUpperMOE_UncUpper));
            AssertIsValidView(section);
        }
    }
}
