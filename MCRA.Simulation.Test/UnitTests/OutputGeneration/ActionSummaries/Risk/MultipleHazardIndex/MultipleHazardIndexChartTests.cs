using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, MultipleHazardIndex
    /// </summary>
    [TestClass]
    public class MultipleHazardIndexChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleHazardIndexHeatMapCreator_TestCreateSingle1() {
            var boxLow = 1000;
            var boxHigh = 10000;
            var boxMedian = 5000;
            var wiskerLow = 1;
            var wiskerHigh = 30000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var hiStatistics = new List<HazardIndexRecord>() {
                createHazardIndexRecord(boxLow, boxHigh, boxMedian, wiskerLow, wiskerHigh, percentages)
            };

            var leftMargin = 0.00001;
            var rightMargin = 100000;
            var thresholdHazardIndex = 1D;
            var section = new SingleHazardIndexSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                HazardIndexRecords = hiStatistics,
                ThresholdHazardIndex = thresholdHazardIndex,
            };

            var chart = new SingleHazardIndexHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateSingle1");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleHazardIndexHeatMapCreator_TestCreateSingle2() {
            var boxLow = 104.023576759244;
            var boxHigh = 1000;
            var boxMedian = 5000;
            var wiskerLow = 88.2335115103254;
            var wiskerHigh = 9000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var hiStatistics = new List<HazardIndexRecord>() {
                createHazardIndexRecord(boxLow, boxHigh, boxMedian, wiskerLow, wiskerHigh, percentages)
            };

            var leftMargin = 0.00001;
            var rightMargin = 10000;
            var thresholdHazardIndex = 1000;
            var section = new SingleHazardIndexSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                HazardIndexRecords = hiStatistics,
                ThresholdHazardIndex = thresholdHazardIndex,
            };

            var chart = new SingleHazardIndexHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateSingle2");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleHazardIndexHeatMapCreator_TestCreateSingle3() {
            var boxLow = 104.023576759244;
            var wiskerHigh = 9500;
            var wiskerLow = 88.2335115103254;
            var boxHigh = 9000;
            var percentages = new double[] { 2.5, 50, 97.5 };
            var hiStatistics = new List<HazardIndexRecord>() {
                createHazardIndexRecord(boxLow, boxHigh, double.NaN, wiskerLow, wiskerHigh, percentages)
            };

            var leftMargin = 0.00001;
            var rightMargin = 10000;
            var thresholdHazardIndex = 200;
            var section = new SingleHazardIndexSection() {
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                HazardIndexRecords = hiStatistics,
                ThresholdHazardIndex = thresholdHazardIndex,
            };

            var chart = new SingleHazardIndexHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateSingle3");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardIndexHeatMapCreator_TestCreateMultipleNominalInverseFalse() {
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
            var section = new MultipleHazardIndexSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: false,
                useIntraSpeciesFactor: false,
                isCumulative: false,
                onlyCumulativeOutput: false
            );

            var chartCreator = new MultipleHazardIndexHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleNominalFalse");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardIndexHeatMapCreator_TestCreateMultipleNominalInverseTrue () {
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
            var section = new MultipleHazardIndexSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: true,
                useIntraSpeciesFactor: false,
                isCumulative: false,
                onlyCumulativeOutput: false
            );

            var chartCreator = new MultipleHazardIndexHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleNominalTrue");
        }
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardIndexHeatMapCreator_TestCreateMultipleFalseUncertain() {
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
            var section = new MultipleHazardIndexSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
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
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    isInverseDistribution: false,
                    uncertaintyLowerBound: 2.5,
                    uncertaintyUpperBound: 97.5,
                    isCumulative: false);
            }

            var chartCreator = new MultipleHazardIndexHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleFalseUncertain");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardIndexHeatMapCreator_TestCreateMultipleTrueUncertain() {
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
            var section = new MultipleHazardIndexSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
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
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    isInverseDistribution: false,
                    uncertaintyLowerBound: 2.5,
                    uncertaintyUpperBound: 97.5,
                    isCumulative: false);
            }

            var chartCreator = new MultipleHazardIndexHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chartCreator, "TestCreateMultipleTrueUncertain");
        }

        private static HazardIndexRecord createHazardIndexRecord(
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
            var record = new HazardIndexRecord() {
                HazardIndexPercentiles = percentiles,
            };
            return record;
        }
    }
}