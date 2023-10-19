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
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRouteType.Dietary);

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
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffectsBySubstance)
            };
            var section = new MultipleHazardExposureRatioSection() { };
            section.Summarize(
                new List<TargetUnit> { targetUnit },
                individualEffectsBySubstanceCollections,
                individualEffects,
                substances,
                null,
                1,
                90,
                RiskMetricType.MarginOfExposure,
                RiskMetricCalculationType.RPFWeighted,
                5,
                10,
                false,
                true
            );
            section.SummarizeUncertain(
                new List<TargetUnit> { targetUnit },
                substances,
                individualEffectsBySubstanceCollections,
                individualEffects,
                RiskMetricCalculationType.RPFWeighted,
                false,
                2.5,
                97.5,
                true);

            Assert.AreEqual(11, section.RiskRecords.SelectMany(c => c.Records).Count());
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records.First().RiskP50UncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records.First().PLowerRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records.First().PUpperRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records.First().PLowerRiskUncLower));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records.First().PUpperRiskUncUpper));
            AssertIsValidView(section);
        }
    }
}
