using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class MultipleRiskRatioHeatMapCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        [DataRow(2, RiskMetricType.ExposureHazardRatio)]
        [DataRow(2, RiskMetricType.HazardExposureRatio)]
        [DataRow(20, RiskMetricType.ExposureHazardRatio)]
        [DataRow(20, RiskMetricType.HazardExposureRatio)]
        public void MultipleRiskRatioHeatMapCreator_TestCreateMultipleNominal(
            int numSubstances,
            RiskMetricType riskMetric
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var section = fakeMultipleRiskRatioSection(numSubstances, riskMetric, false, false, random);
            var chart = new MultipleRiskRatioHeatMapCreator(section, targetUnit, false);
            RenderChart(chart, $"TestCreateMultipleNominal_{riskMetric}_{numSubstances}");
            AssertIsValidView(section);
        }

        [TestMethod]
        [DataRow(2, RiskMetricType.ExposureHazardRatio)]
        [DataRow(2, RiskMetricType.HazardExposureRatio)]
        [DataRow(20, RiskMetricType.ExposureHazardRatio)]
        [DataRow(20, RiskMetricType.HazardExposureRatio)]
        public void MultipleRiskRatioHeatMapCreator_TestCreateMultipleNominalInverse(
            int numSubstances,
            RiskMetricType riskMetric
        ) {
            var seed = 1;
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var random = new McraRandomGenerator(seed);
            var section = fakeMultipleRiskRatioSection(numSubstances, riskMetric, false, true, random);
            var chart = new MultipleRiskRatioHeatMapCreator(section, targetUnit, false);
            RenderChart(chart, $"TestCreateMultipleNominalInverse_{riskMetric}_{numSubstances}");
            AssertIsValidView(section);
        }

        [TestMethod]
        [DataRow(2, RiskMetricType.ExposureHazardRatio)]
        [DataRow(2, RiskMetricType.HazardExposureRatio)]
        [DataRow(20, RiskMetricType.ExposureHazardRatio)]
        [DataRow(20, RiskMetricType.HazardExposureRatio)]
        public void MultipleRiskRatioHeatMapCreator_TestCreateMultipleUncertain(
            int numSubstances, 
            RiskMetricType riskMetric
        ) {
            var seed = 1;
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var random = new McraRandomGenerator(seed);
            var section = fakeMultipleRiskRatioSection(numSubstances, riskMetric, true, false, random);
            var chart = new MultipleRiskRatioHeatMapCreator(section, targetUnit, true);
            RenderChart(chart, $"TestCreateMultipleUncertain_{riskMetric}_{numSubstances}");
            AssertIsValidView(section);
        }

        [TestMethod]
        [DataRow(2, RiskMetricType.ExposureHazardRatio)]
        [DataRow(2, RiskMetricType.HazardExposureRatio)]
        [DataRow(20, RiskMetricType.ExposureHazardRatio)]
        [DataRow(20, RiskMetricType.HazardExposureRatio)]
        public void MultipleRiskRatioHeatMapCreator_TestCreateMultipleUncertainInverse(
            int numSubstances,
            RiskMetricType riskMetric
        ) {
            var seed = 1;
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var random = new McraRandomGenerator(seed);
            var section = fakeMultipleRiskRatioSection(numSubstances, riskMetric, true, false, random);
            var chart = new MultipleRiskRatioHeatMapCreator(section, targetUnit, true);
            RenderChart(chart, $"TestCreateMultipleUncertainInverse_{riskMetric}_{numSubstances}");
            AssertIsValidView(section);
        }

        private static MultipleRiskRatioSection fakeMultipleRiskRatioSection(
            int numSubstances,
            RiskMetricType riskMetric,
            bool uncertainty,
            bool isInverseDistribution,
            IRandom random
        ) {
            var substances = FakeSubstancesGenerator.Create(numSubstances);
            var individuals = FakeIndividualsGenerator.CreateSimulated(10, 2, random);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var individualEffects = FakeIndividualEffectsGenerator
                .Create(
                    individuals,
                    substances,
                    random
                );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                (targetUnit.Target, individualEffects)
            };
            var section = new MultipleRiskRatioSection();
            section.Summarize(
                null,
                individualEffectsBySubstanceCollections,
                [targetUnit],
                substances,
                null,
                confidenceInterval: 5,
                threshold: 100,
                riskMetricType: riskMetric,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                null,
                leftMargin: 0.01,
                rightMargin: 10000,
                isInverseDistribution: isInverseDistribution,
                isCumulative: false,
                stratifier: null,
                skipPrivacySensitiveOutputs: false
            );

            if (uncertainty) {
                for (int i = 0; i < 100; i++) {
                    foreach (var ie in individualEffects.Values) {
                        foreach (var item in ie) {
                            var x = .90 + (random.NextDouble() / 5D);
                            item.ExposureHazardRatio *= x;
                            item.HazardExposureRatio *= 1/x;
                            item.Exposure *= x;
                        }
                    }
                    var individualEffectsBySubstanceCollectionsUncertain = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                        (targetUnit.Target, individualEffects)
                    };
                    section.SummarizeUncertain(
                        [targetUnit],
                        substances: substances,
                        individualEffectsBySubstanceCollections: individualEffectsBySubstanceCollectionsUncertain,
                        individualEffects: null,
                        riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                        isInverseDistribution: false,
                        uncertaintyLowerBound: 2.5,
                        uncertaintyUpperBound: 97.5,
                        isCumulative: false,
                        stratifier: null
                    );
                }
            }
            return section;
        }
    }
}