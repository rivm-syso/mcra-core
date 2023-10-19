using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class MultipleExposureHazardRatioChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleExposureHazardRatioHeatMapCreator_TestCreateSingle1() {
            var boxLow = 1000;
            var boxHigh = 10000;
            var boxMedian = 5000;
            var wiskerLow = 1;
            var wiskerHigh = 30000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var riskStatistics = new List<(ExposureTarget target, List<SubstanceRiskDistributionRecord> records)>{
                (
                new ExposureTarget(),
                new List<SubstanceRiskDistributionRecord>{ createExposureHazardRatioRecord(boxLow, boxHigh, boxMedian, wiskerLow, wiskerHigh, percentages) }
                )
            };

            var leftMargin = 0.00001;
            var rightMargin = 100000;
            var threshold = 1D;
            var section = new SingleExposureHazardRatioSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                RiskRecords = riskStatistics,
                Threshold = threshold
            };

            var chart = new SingleExposureHazardRatioHeatMapCreator(section, true);
            RenderChart(chart, "TestCreateSingle1");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleExposureHazardRatioHeatMapCreator_TestCreateSingle2() {
            var boxLow = 104.023576759244;
            var boxHigh = 1000;
            var boxMedian = 5000;
            var wiskerLow = 88.2335115103254;
            var wiskerHigh = 9000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var riskStatistics = new List<(ExposureTarget target, List<SubstanceRiskDistributionRecord> records)>{
                (
                null,
                new List<SubstanceRiskDistributionRecord>{ createExposureHazardRatioRecord(boxLow, boxHigh, boxMedian, wiskerLow, wiskerHigh, percentages) }
                )
            };
            var leftMargin = 0.00001;
            var rightMargin = 10000;
            var threshold = 1000;
            var section = new SingleExposureHazardRatioSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                RiskRecords = riskStatistics,
                Threshold = threshold
            };

            var chart = new SingleExposureHazardRatioHeatMapCreator(section, true);
            RenderChart(chart, "TestCreateSingle2");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleExposureHazardRatioHeatMapCreator_TestCreateSingle3() {
            var boxLow = 104.023576759244;
            var wiskerHigh = 9500;
            var wiskerLow = 88.2335115103254;
            var boxHigh = 9000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var riskStatistics = new List<(ExposureTarget target, List<SubstanceRiskDistributionRecord> records)>{(
                null,
                new List<SubstanceRiskDistributionRecord>{ createExposureHazardRatioRecord(boxLow, boxHigh, double.NaN, wiskerLow, wiskerHigh, percentages) }
            )};
            var leftMargin = 0.00001;
            var rightMargin = 10000;
            var threshold = 200;
            var section = new SingleExposureHazardRatioSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                RiskRecords = riskStatistics,
                Threshold = threshold,
            };

            var chart = new SingleExposureHazardRatioHeatMapCreator(section, true);
            RenderChart(chart, "TestCreateSingle3");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureHazardRatioHeatMapCreator_TestCreateMultipleNominalInverseFalse() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(20);
            var individuals = MockIndividualsGenerator.Create(10, 2, random);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRouteType.Dietary);
            var individualEffects = MockIndividualEffectsGenerator
                .Create(
                    individuals,
                    substances,
                    random
                );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                (targetUnit.Target, individualEffects)
            };
            var section = new MultipleExposureHazardRatioSection();
            section.Summarize(
                new List<TargetUnit> { targetUnit },
                individualEffectsBySubstanceCollections: individualEffectsBySubstanceCollections,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: false,
                useIntraSpeciesFactor: false,
                isCumulative: false
            );

            var chartCreator = new MultipleExposureHazardRatioHeatMapCreator(section, targetUnit, false);
            RenderChart(chartCreator, "TestCreateMultipleNominalFalse");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureHazardRatioHeatMapCreator_TestCreateMultipleNominalInverseTrue() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(20);
            var individuals = MockIndividualsGenerator.Create(10, 2, random);

            var individualEffects = MockIndividualEffectsGenerator
                .Create(
                    individuals,
                    substances,
                    random
                );
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRouteType.Dietary);
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new MultipleExposureHazardRatioSection();
            section.Summarize(
                new List<TargetUnit> { targetUnit },
                individualEffectsBySubstanceCollections: individualEffectsBySubstanceCollections,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: true,
                useIntraSpeciesFactor: false,
                isCumulative: false
            );

            var chartCreator = new MultipleExposureHazardRatioHeatMapCreator(section, targetUnit, false);
            RenderChart(chartCreator, "TestCreateMultipleNominalTrue");
        }
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureHazardRatioHeatMapCreator_TestCreateMultipleFalseUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(20);
            var individuals = MockIndividualsGenerator.Create(10, 2, random);

            var individualEffects = MockIndividualEffectsGenerator
                .Create(
                    individuals,
                    substances,
                    random
                );
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRouteType.Dietary);
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new MultipleExposureHazardRatioSection();
            section.Summarize(
                new List<TargetUnit> { targetUnit },
                individualEffectsBySubstanceCollections: individualEffectsBySubstanceCollections,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: false,
                useIntraSpeciesFactor: false,
                isCumulative: false
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var individualEffectsBySubstanceCollectionsUncertains = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                    (   targetUnit.Target, substanceIndividualEffectsUncertains)
                };
                section.SummarizeUncertain(
                    new List<TargetUnit> { targetUnit },
                    substances: substances,
                    individualEffectsBySubstanceCollections: individualEffectsBySubstanceCollectionsUncertains,
                    individualEffects: null,
                    isInverseDistribution: false,
                    uncertaintyLowerBound: 2.5,
                    uncertaintyUpperBound: 97.5,
                    isCumulative: false);
            }

            var chartCreator = new MultipleExposureHazardRatioHeatMapCreator(section, targetUnit, true);
            RenderChart(chartCreator, "TestCreateMultipleFalseUncertain");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureHazardRatioHeatMapCreator_TestCreateMultipleTrueUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(20);
            var individuals = MockIndividualsGenerator.Create(10, 2, random);

            var individualEffects = MockIndividualEffectsGenerator
                .Create(
                    individuals,
                    substances,
                    random
                );
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRouteType.Dietary);
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new MultipleExposureHazardRatioSection();
            section.Summarize(
                new List<TargetUnit> { targetUnit },
                individualEffectsBySubstanceCollections: individualEffectsBySubstanceCollections,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: true,
                useIntraSpeciesFactor: false,
                isCumulative: false
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var individualEffectsBySubstanceCollectionsUncertains = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)>{
                    (targetUnit.Target, substanceIndividualEffectsUncertains)
                };
                section.SummarizeUncertain(
                    new List<TargetUnit> { targetUnit },
                    substances: substances,
                    individualEffectsBySubstanceCollections: individualEffectsBySubstanceCollectionsUncertains,
                    individualEffects: null,
                    isInverseDistribution: false,
                    uncertaintyLowerBound: 2.5,
                    uncertaintyUpperBound: 97.5,
                    isCumulative: false);
            }

            var chartCreator = new MultipleExposureHazardRatioHeatMapCreator(section, targetUnit, true);
            RenderChart(chartCreator, "TestCreateMultipleTrueUncertain");
        }

        private static SubstanceRiskDistributionRecord createExposureHazardRatioRecord(
            double boxLow,
            double boxHigh,
            double boxMedian,
            double wiskerLow,
            double wiskerHigh,
            double[] percentages
        ) {
            var percentiles = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = new List<double> { boxLow, boxMedian, boxHigh },
            };
            for (int i = 0; i < 10; i++) {
                percentiles.AddUncertaintyValues(new List<double> { wiskerLow, double.NaN, wiskerHigh });
            }
            var record = new SubstanceRiskDistributionRecord() {
                RiskPercentiles = percentiles,
            };
            return record;
        }
    }
}