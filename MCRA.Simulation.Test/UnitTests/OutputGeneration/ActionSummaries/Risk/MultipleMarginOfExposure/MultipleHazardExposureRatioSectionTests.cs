using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var individuals = FakeIndividualsGenerator.CreateSimulated(100, 1, random);
            var substances = FakeSubstancesGenerator.Create(10);
            var individualEffectsBySubstance = new Dictionary<Compound, List<IndividualEffect>>();
            var individualEffects = new List<IndividualEffect>();
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);

            foreach (var substance in substances) {
                individualEffectsBySubstance[substance] = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }

            for (int i = 0; i < 100; i++) {
                individualEffects.Add(new IndividualEffect() {
                    SimulatedIndividual = individualEffectsBySubstance[substances.First()].ElementAt(i).SimulatedIndividual,
                    CriticalEffectDose = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose,
                    Exposure = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose / individualEffectsBySubstance[substances.First()].ElementAt(i).HazardExposureRatio,
                    HazardExposureRatio = individualEffectsBySubstance[substances.First()].ElementAt(i).HazardExposureRatio
                });
            }
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffectsBySubstance)
            };
            var section = new MultipleHazardExposureRatioSection() { };
            section.Summarize(
                [targetUnit],
                individualEffectsBySubstanceCollections,
                individualEffects,
                substances,
                null,
                1,
                90,
                RiskMetricType.HazardExposureRatio,
                RiskMetricCalculationType.RPFWeighted,
                5,
                10,
                false,
                true,
                skipPrivacySensitiveOutputs: false
            );
            section.SummarizeUncertain(
                [targetUnit],
                substances,
                individualEffectsBySubstanceCollections,
                individualEffects,
                RiskMetricCalculationType.RPFWeighted,
                false,
                2.5,
                97.5,
                true
            );

            Assert.AreEqual(11, section.RiskRecords.SelectMany(c => c.Records).Count());
            Assert.IsFalse(double.IsNaN(section.RiskRecords[0].Records.First().RiskP50UncP50));
            Assert.IsFalse(double.IsNaN(section.RiskRecords[0].Records.First().PLowerRiskUncP50));
            Assert.IsFalse(double.IsNaN(section.RiskRecords[0].Records.First().PUpperRiskUncP50));
            Assert.IsFalse(double.IsNaN(section.RiskRecords[0].Records.First().PLowerRiskUncLower));
            Assert.IsFalse(double.IsNaN(section.RiskRecords[0].Records.First().PUpperRiskUncUpper));
            AssertIsValidView(section);
        }
    }
}
