using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.OpexProductDefinitions.Dto;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class SingleStratifiedRiskRatioHeatMapCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio)]
        [DataRow(RiskMetricType.HazardExposureRatio)]
        public void SingleStratifiedRiskRatioHeatMapCreator_TestCreateSingleNominal(RiskMetricType riskMetric) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var records = new List<RiskDistributionRecordBase<SubstanceContributorKey>>();
            var section = fakeMultipleHazardExposureRatioSection(riskMetric, false, false, random);
            var chart = new SingleStratifiedRiskRatioHeatMapCreator(section, targetUnit, false);
            RenderChart(chart, $"TestCreateSingleNominal_{riskMetric}");
        }

        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio)]
        [DataRow(RiskMetricType.HazardExposureRatio)]
        public void SingleStratifiedRiskRatioHeatMapCreator_TestCreateSingleUnc(RiskMetricType riskMetric) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var records = new List<RiskDistributionRecordBase<SubstanceContributorKey>>();
            var section = fakeMultipleHazardExposureRatioSection(riskMetric, false, true, random);
            var chart = new SingleStratifiedRiskRatioHeatMapCreator(section, targetUnit, true);
            RenderChart(chart, $"TestCreateSingleUnc_{riskMetric}");
        }

        private static SingleRiskRatioSection fakeMultipleHazardExposureRatioSection(
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            bool uncertainty,
            IRandom random
        ) {
            var substance = FakeSubstancesGenerator.Create(1).First();
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individuals = FakeIndividualsGenerator.Create(50, 2, false, properties, random);
            var simulatedIndividuals = individuals.Select(c => new SimulatedIndividual(c, c.Id)).ToList();
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var individualEffects = FakeIndividualEffectsGenerator
                .Create(
                    simulatedIndividuals,
                    [substance],
                    random
                );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                (targetUnit.Target, individualEffects)
            };
            var section = new SingleRiskRatioSection();
            var outputStratifiers = new List<ISimplePopulationStratifier>() { new GenderStratifier() };
            section.Summarize(
                individualEffects.First().Value,
                targetUnit: targetUnit,
                substance: substance,
                effect: new Effect() { Code = "Fake" },
                confidenceInterval: 95d,
                threshold: 100,
                riskMetricType: riskMetricType,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                referenceDose: new HazardCharacterisationModel(),
                leftMargin: 0.01,
                rightMargin: 10000,
                isInverseDistribution: isInverseDistribution,
                isCumulative: false,
                stratifier: new PopulationStratifier(outputStratifiers),
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
                        stratifier: new PopulationStratifier(outputStratifiers)
                    );
                }
            }

            return section;
        }
    }
}