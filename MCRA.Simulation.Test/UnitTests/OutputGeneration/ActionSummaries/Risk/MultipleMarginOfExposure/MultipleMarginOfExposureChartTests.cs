using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, MultipleMarginOfExposure
    /// </summary>
    [TestClass]
    public class MultipleMarginOfExposureChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleMarginOfExposureHeatMapCreator_TestCreateSingle1() {
            var xLow = 0.01;
            var xHigh = 100000;
            var boxLow = 1000;
            var wiskerHigh = 30000;
            var wiskerLow = 1;
            var boxHigh = 10000;
            MarginOfExposureRecord record = createMarginOfExposureRecord(
                boxLow,
                boxHigh,
                double.NaN,
                wiskerLow,
                wiskerHigh,
                new double[] { 2.5, 50, 97.5 }
            );
            var section = new SingleMarginOfExposureSection() {
                LeftMargin = xLow,
                RightMargin = xHigh,
                MoeRecords = new List<MarginOfExposureRecord>() { record },
                ThresholdMarginOfExposure = 1000,
                CED = 43.8
            };
            var chart = new SingleMarginOfExposureHeatMapCreator(section, true, "mg/kg bw/Dday");
            RenderChart(chart, "ChartHeatMap4");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleMarginOfExposureHeatMapCreator_TestCreateSingle2() {
            var xLow = 0.01;
            var xHigh = 10000;
            var boxLow = 104.023576759244;
            var wiskerHigh = 9000;
            var wiskerMedian = 5000;
            var wiskerLow = 88.2335115103254;
            var boxHigh = 1000;
            var record = createMarginOfExposureRecord(boxLow, boxHigh, wiskerMedian, wiskerLow, wiskerHigh, new double[] { 2.5, 50, 97.5 });
            var section = new SingleMarginOfExposureSection() {
                MoeRecords = new List<MarginOfExposureRecord>() { record },
                LeftMargin = xLow,
                RightMargin = xHigh,
                ThresholdMarginOfExposure = 10,
            };
            var chart = new SingleMarginOfExposureHeatMapCreator(section, true, "mg/kg bw/Dday");
            RenderChart(chart, "TestCreateSingle2");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void SingleMarginOfExposureHeatMapCreator_TestCreateSingle3() {
            var xLow = 0.001;
            var xHigh = 10000;
            var boxLow = 104.023576759244;
            var wiskerHigh = 9500;
            var wiskerLow = 88.2335115103254;
            var boxHigh = 9000;
            var record = createMarginOfExposureRecord(boxLow, boxHigh, double.NaN, wiskerLow, wiskerHigh, new double[] { 2.5, 50, 97.5 });
            var section = new SingleMarginOfExposureSection() {
                MoeRecords = new List<MarginOfExposureRecord>() { record },
                LeftMargin = xLow,
                RightMargin = xHigh,
                ThresholdMarginOfExposure = 200,
            };
            var chart = new SingleMarginOfExposureHeatMapCreator(section, true, "mg/kg bw/Dday");
            RenderChart(chart, "TestCreateSingle3");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleMarginOfExposureHeatMapCreator_TestCreateMultipleNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleMarginOfExposureSection(false, false, random);
            var chart = new MultipleMarginOfExposureHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleNominal");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleMarginOfExposureHeatMapCreator_TestCreateMultipleNominalInverse() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleMarginOfExposureSection(false, true, random);
            var chart = new MultipleMarginOfExposureHeatMapCreator(section, false, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleNominalInverse");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleMarginOfExposureHeatMapCreator_TestCreateMultipleUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleMarginOfExposureSection(true, false, random);
            var chart = new MultipleMarginOfExposureHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleUncertain");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void MultipleMarginOfExposureHeatMapCreator_TestCreateMultipleUncertainInverse() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = mockMultipleMarginOfExposureSection(true, false, random);
            var chart = new MultipleMarginOfExposureHeatMapCreator(section, true, "mg/kg bw/day");
            RenderChart(chart, "TestCreateMultipleUncertainInverse");
            AssertIsValidView(section);
        }

        private static MultipleMarginOfExposureSection mockMultipleMarginOfExposureSection(
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

            var section = new MultipleMarginOfExposureSection();
            section.SummarizeMultipleSubstances(
                individualEffects,
                null,
                substances,
                null,
                thresholdMarginOfExposure: 100,
                confidenceInterval: 5,
                healthEffectType: HealthEffectType.Risk,
                RiskMetricCalculationType.RPFWeighted,
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

        private static MarginOfExposureRecord createMarginOfExposureRecord(
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
            var record = new MarginOfExposureRecord() {
                MarginOfExposurePercentiles = percentiles,
            };
            return record;
        }
    }
}