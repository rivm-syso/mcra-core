using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, MultipleThresholdExposureRatio
    /// </summary>
    [TestClass]
    public class MultipleHazardExposureRatioChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleHazardExposureRatioHeatMapCreator_TestCreateSingle1() {
            var xLow = 0.01;
            var xHigh = 100000;
            var boxLow = 1000;
            var wiskerHigh = 30000;
            var wiskerLow = 1;
            var boxHigh = 10000;
            SubstanceRiskDistributionRecord record = createHazardExposureRatioRecord(
                boxLow,
                boxHigh,
                double.NaN,
                wiskerLow,
                wiskerHigh,
                new double[] { 2.5, 50, 97.5 }
            );
            var section = new SingleHazardExposureRatioSection() {
                LeftMargin = xLow,
                RightMargin = xHigh,
                RiskRecords = new List<SubstanceRiskDistributionRecord>() { record },
                Threshold = 1000,
                CED = 43.8
            };
            var chart = new SingleHazardExposureRatioHeatMapCreator(section, true, "mg/kg bw/Dday");
            RenderChart(chart, "ChartHeatMap4");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleHazardExposureRatioHeatMapCreator_TestCreateSingle2() {
            var xLow = 0.01;
            var xHigh = 10000;
            var boxLow = 104.023576759244;
            var wiskerHigh = 9000;
            var wiskerMedian = 5000;
            var wiskerLow = 88.2335115103254;
            var boxHigh = 1000;
            var record = createHazardExposureRatioRecord(boxLow, boxHigh, wiskerMedian, wiskerLow, wiskerHigh, new double[] { 2.5, 50, 97.5 });
            var section = new SingleHazardExposureRatioSection() {
                RiskRecords = new List<SubstanceRiskDistributionRecord>() { record },
                LeftMargin = xLow,
                RightMargin = xHigh,
                Threshold = 10,
            };
            var chart = new SingleHazardExposureRatioHeatMapCreator(section, true, "mg/kg bw/Dday");
            RenderChart(chart, "TestCreateSingle2");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleHazardExposureRatioHeatMapCreator_TestCreateSingle3() {
            var xLow = 0.001;
            var xHigh = 10000;
            var boxLow = 104.023576759244;
            var wiskerHigh = 9500;
            var wiskerLow = 88.2335115103254;
            var boxHigh = 9000;
            var record = createHazardExposureRatioRecord(boxLow, boxHigh, double.NaN, wiskerLow, wiskerHigh, new double[] { 2.5, 50, 97.5 });
            var section = new SingleHazardExposureRatioSection() {
                RiskRecords = new List<SubstanceRiskDistributionRecord>() { record },
                LeftMargin = xLow,
                RightMargin = xHigh,
                Threshold = 200,
            };
            var chart = new SingleHazardExposureRatioHeatMapCreator(section, true, "mg/kg bw/Dday");
            RenderChart(chart, "TestCreateSingle3");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardExposureRatioHeatMapCreator_TestCreateMultipleNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleHazardExposureRatioSection(false, false, random);
            var chart = new MultipleHazardExposureRatioHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleNominal");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardExposureRatioHeatMapCreator_TestCreateMultipleNominalInverse() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleHazardExposureRatioSection(false, true, random);
            var chart = new MultipleHazardExposureRatioHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleNominalInverse");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardExposureRatioHeatMapCreator_TestCreateMultipleUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleHazardExposureRatioSection(true, false, random);
            var chart = new MultipleHazardExposureRatioHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleUncertain");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleHazardExposureRatioHeatMapCreator_TestCreateMultipleUncertainInverse() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleHazardExposureRatioSection(true, false, random);
            var chart = new MultipleHazardExposureRatioHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleUncertainInverse");
            AssertIsValidView(section);
        }

        private static MultipleHazardExposureRatioSection mockMultipleHazardExposureRatioSection(
            bool uncertainty,
            bool isInverseDistribution,
            IRandom random
        ) {
            var substances = MockSubstancesGenerator.Create(50);
            var individuals = MockIndividualsGenerator.Create(10, 2, random);

            var individualEffects = MockIndividualEffectsGenerator
                .Create(
                    individuals,
                    substances,
                    random
                );

            var section = new MultipleHazardExposureRatioSection();
            section.SummarizeMultipleSubstances(
                individualEffects,
                null,
                substances,
                null,
                threshold: 100,
                confidenceInterval: 5,
                healthEffectType: HealthEffectType.Risk,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.MarginOfExposure,
                leftMargin: 0.01,
                rightMargin: 10000,
                isInverseDistribution: isInverseDistribution,
                isCumulative: false,
                onlyCumulativeOutput: false
            );

            if (uncertainty) {
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
            }
            return section;
        }

        private static SubstanceRiskDistributionRecord createHazardExposureRatioRecord(
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