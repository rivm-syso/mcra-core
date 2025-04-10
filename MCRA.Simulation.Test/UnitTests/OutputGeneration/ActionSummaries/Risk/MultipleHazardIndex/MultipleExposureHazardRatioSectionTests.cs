﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class MultipleExposureHazardRatioSectionTests : SectionTestBase {

        /// <summary>
        /// Test summarize with uncertainty and rendering of section view.
        /// </summary>
        [TestMethod]
        public void MultipleExposureHazardRatioSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(10);
            var effects = FakeEffectsGenerator.Create(1);
            var individuals = FakeIndividualsGenerator.CreateSimulated(100, 1, random);
            var individualEffectsBySubstance = new Dictionary<Compound, List<IndividualEffect>>();
            var cumulativeExposureHazardRatio = new List<IndividualEffect>();
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            foreach (var substance in substances) {
                var individualEffects = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);
                individualEffectsBySubstance[substance] = individualEffects;
            }

            for (int i = 0; i < 100; i++) {
                cumulativeExposureHazardRatio.Add(new IndividualEffect() {
                    SimulatedIndividual = individualEffectsBySubstance[substances.First()].ElementAt(i).SimulatedIndividual,
                    CriticalEffectDose = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose,
                    Exposure = individualEffectsBySubstance[substances.First()].ElementAt(i).CriticalEffectDose / individualEffectsBySubstance[substances.First()].ElementAt(i).HazardExposureRatio,
                });
            }
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                (targetUnit.Target, individualEffectsBySubstance)
            };

            var section = new MultipleExposureHazardRatioSection();
            section.Summarize(
                [targetUnit],
                individualEffectsBySubstanceCollections,
                cumulativeExposureHazardRatio,
                substances,
                effects.First(),
                RiskMetricCalculationType.RPFWeighted,
                RiskMetricType.ExposureHazardRatio,
                90,
                1,
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
                cumulativeExposureHazardRatio,
                false,
                2.5,
                97.5,
                true
            );

            Assert.AreEqual(11, section.RiskRecords.SelectMany(c => c.Records).Count());
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records[1].RiskP50UncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records[1].PLowerRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records[1].PUpperRiskUncP50));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records[1].PLowerRiskUncLower));
            Assert.IsTrue(!double.IsNaN(section.RiskRecords[0].Records[1].PUpperRiskUncUpper));
            RenderView(section, filename: "MultipleHExposureHazardRatioSection_TestSummarize.html");
        }
    }
}
