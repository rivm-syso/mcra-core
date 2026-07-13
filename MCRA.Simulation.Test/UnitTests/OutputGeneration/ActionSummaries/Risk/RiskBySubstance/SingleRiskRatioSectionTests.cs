using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class SingleRiskRatioSectionTests : SectionTestBase {

        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio, false, false, false)]
        [DataRow(RiskMetricType.HazardExposureRatio, false, false, false)]
        [DataRow(RiskMetricType.ExposureHazardRatio, true, false, false)]
        [DataRow(RiskMetricType.HazardExposureRatio, true, false, false)]
        [DataRow(RiskMetricType.ExposureHazardRatio, false, false, true)]
        [DataRow(RiskMetricType.HazardExposureRatio, false, false, true)]
        [DataRow(RiskMetricType.ExposureHazardRatio, true, false, true)]
        [DataRow(RiskMetricType.HazardExposureRatio, true, false, true)]
        [DataRow(RiskMetricType.ExposureHazardRatio, false, true, false)]
        [DataRow(RiskMetricType.HazardExposureRatio, false, true, false)]
        [DataRow(RiskMetricType.ExposureHazardRatio, true, true, false)]
        [DataRow(RiskMetricType.HazardExposureRatio, true, true, false)]
        [DataRow(RiskMetricType.ExposureHazardRatio, false, true, true)]
        [DataRow(RiskMetricType.HazardExposureRatio, false, true, true)]
        [DataRow(RiskMetricType.ExposureHazardRatio, true, true, true)]
        [DataRow(RiskMetricType.HazardExposureRatio, true, true, true)]
        public void SingleRiskRatioSection_TestSummarize(
            RiskMetricType riskMetric,
            bool isInverseDistribution,
            bool stratify,
            bool uncertainty
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substance = FakeSubstancesGenerator.Create(1).First();
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var fractionZeroExposure = random.NextDouble();
            var individualEffects = fakeIndividualEffects(random, substance, targetUnit, fractionZeroExposure);
            var section = new SingleRiskRatioSection();
            var stratifier = stratify
                ? new PopulationStratifier([new GenderStratifier()])
                : null;
            section.Summarize(
                individualEffects.First().Value,
                targetUnit: targetUnit,
                substance: substance,
                effect: new Effect() { Code = "Fake" },
                confidenceInterval: 95d,
                threshold: 100,
                riskMetricType: riskMetric,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                referenceDose: new HazardCharacterisationModel(),
                leftMargin: 0.01,
                rightMargin: 10000,
                isInverseDistribution: isInverseDistribution,
                isCumulative: false,
                stratifier: stratifier,
                skipPrivacySensitiveOutputs: false
            );

            if (uncertainty) {
                for (int i = 0; i < 100; i++) {
                    foreach (var ie in individualEffects.Values) {
                        foreach (var item in ie) {
                            var x = .90 + (random.NextDouble() / 5D);
                            item.ExposureHazardRatio *= x;
                            item.HazardExposureRatio *= 1 / x;
                            item.Exposure *= x;
                        }
                    }
                    var individualEffectsBySubstanceCollectionsUncertain = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                        (targetUnit.Target, individualEffects)
                    };
                    section.SummarizeUncertain(
                        substance: substance,
                        individualEffects: individualEffects.First().Value,
                        riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                        isInverseDistribution: false,
                        uncertaintyLowerBound: 2.5,
                        uncertaintyUpperBound: 97.5,
                        isCumulative: false,
                        stratifier: stratifier
                    );
                }
            }

            AssertIsValidView(section);
        }

        private static Dictionary<Compound, List<IndividualEffect>> fakeIndividualEffects(
            IRandom random,
            Compound substance,
            TargetUnit targetUnit,
            double fractionZeroExposure
        ) {
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individuals = FakeIndividualsGenerator.Create(50, 2, false, properties, random);
            var simulatedIndividuals = individuals.Select(c => new SimulatedIndividual(c, c.Id)).ToList();
            var individualEffects = FakeIndividualEffectsGenerator
                .Create(
                    simulatedIndividuals,
                    [substance],
                    random,
                    fractionZeroExposure: [fractionZeroExposure]
                );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                (targetUnit.Target, individualEffects)
            };
            return individualEffects;
        }
    }
}