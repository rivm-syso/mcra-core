using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, MultipleExposureThresholdRatio
    /// </summary>
    [TestClass]
    public class MultipleExposureThresholdRatioChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleExposureThresholdRatioHeatMapCreator_TestCreateSingle1() {
            var boxLow = 1000;
            var boxHigh = 10000;
            var boxMedian = 5000;
            var wiskerLow = 1;
            var wiskerHigh = 30000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var riskStatistics = new List<SubstanceRiskDistributionRecord>() {
                createExposureThresholdRatioRecord(boxLow, boxHigh, boxMedian, wiskerLow, wiskerHigh, percentages)
            };

            var leftMargin = 0.00001;
            var rightMargin = 100000;
            var threshold = 1D;
            var section = new SingleExposureThresholdRatioSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                RiskRecords = riskStatistics,
                Threshold = threshold,
            };

            var chart = new SingleExposureThresholdRatioHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateSingle1");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleExposureThresholdRatioHeatMapCreator_TestCreateSingle2() {
            var boxLow = 104.023576759244;
            var boxHigh = 1000;
            var boxMedian = 5000;
            var wiskerLow = 88.2335115103254;
            var wiskerHigh = 9000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var riskStatistics = new List<SubstanceRiskDistributionRecord>() {
                createExposureThresholdRatioRecord(boxLow, boxHigh, boxMedian, wiskerLow, wiskerHigh, percentages)
            };

            var leftMargin = 0.00001;
            var rightMargin = 10000;
            var threshold = 1000;
            var section = new SingleExposureThresholdRatioSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                RiskRecords = riskStatistics,
                Threshold = threshold,
            };

            var chart = new SingleExposureThresholdRatioHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateSingle2");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleExposureThresholdRatioHeatMapCreator_TestCreateSingle3() {
            var boxLow = 104.023576759244;
            var wiskerHigh = 9500;
            var wiskerLow = 88.2335115103254;
            var boxHigh = 9000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var riskStatistics = new List<SubstanceRiskDistributionRecord>() {
                createExposureThresholdRatioRecord(boxLow, boxHigh, double.NaN, wiskerLow, wiskerHigh, percentages)
            };

            var leftMargin = 0.00001;
            var rightMargin = 10000;
            var threshold = 200;
            var section = new SingleExposureThresholdRatioSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                RiskRecords = riskStatistics,
                Threshold = threshold,
            };

            var chart = new SingleExposureThresholdRatioHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateSingle3");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureThresholdRatioHeatMapCreator_TestCreateMultipleNominalInverseFalse() {
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
            var section = new MultipleExposureThresholdRatioSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: false,
                useIntraSpeciesFactor: false,
                isCumulative: false,
                onlyCumulativeOutput: false
            );

            var chartCreator = new MultipleExposureThresholdRatioHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleNominalFalse");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureThresholdRatioHeatMapCreator_TestCreateMultipleNominalInverseTrue () {
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
            var section = new MultipleExposureThresholdRatioSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: true,
                useIntraSpeciesFactor: false,
                isCumulative: false,
                onlyCumulativeOutput: false
            );

            var chartCreator = new MultipleExposureThresholdRatioHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleNominalTrue");
        }
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureThresholdRatioHeatMapCreator_TestCreateMultipleFalseUncertain() {
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
            var section = new MultipleExposureThresholdRatioSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: false,
                useIntraSpeciesFactor: false,
                isCumulative: false,
                onlyCumulativeOutput: false
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                section.SummarizeMultipleSubstancesUncertainty(
                    substances: substances,
                    individualEffectsBySubstance: substanceIndividualEffectsUncertains,
                    individualEffects: null,
                    isInverseDistribution: false,
                    uncertaintyLowerBound: 2.5,
                    uncertaintyUpperBound: 97.5,
                    isCumulative: false);
            }

            var chartCreator = new MultipleExposureThresholdRatioHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleFalseUncertain");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleExposureThresholdRatioHeatMapCreator_TestCreateMultipleTrueUncertain() {
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
            var section = new MultipleExposureThresholdRatioSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: true,
                useIntraSpeciesFactor: false,
                isCumulative: false,
                onlyCumulativeOutput: false
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                section.SummarizeMultipleSubstancesUncertainty(
                    substances: substances,
                    individualEffectsBySubstance: substanceIndividualEffectsUncertains,
                    individualEffects: null,
                    isInverseDistribution: false,
                    uncertaintyLowerBound: 2.5,
                    uncertaintyUpperBound: 97.5,
                    isCumulative: false);
            }

            var chartCreator = new MultipleExposureThresholdRatioHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleTrueUncertain");
        }

        private static SubstanceRiskDistributionRecord createExposureThresholdRatioRecord(
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