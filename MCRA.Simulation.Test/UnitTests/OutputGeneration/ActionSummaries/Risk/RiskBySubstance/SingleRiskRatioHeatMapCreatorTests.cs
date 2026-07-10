using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, MultipleThresholdExposureRatio
    /// </summary>
    [TestClass]
    public class SingleRiskRatioHeatMapCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        [DataRow(1, 200, 400, 1000, RiskMetricType.HazardExposureRatio)]
        [DataRow(2, 200, 400, double.PositiveInfinity, RiskMetricType.HazardExposureRatio)]
        [DataRow(3, 200, double.PositiveInfinity, double.PositiveInfinity, RiskMetricType.HazardExposureRatio)]
        [DataRow(1, 0.001, 0.01, 1, RiskMetricType.ExposureHazardRatio)]
        [DataRow(2, 0, 0.01, 1, RiskMetricType.ExposureHazardRatio)]
        [DataRow(3, 0, 0, 1, RiskMetricType.ExposureHazardRatio)]
        public void SingleRiskRatioHeatMapCreator_TestCreateNoUnc(
            int id,
            double boxLow,
            double boxMedian,
            double boxHigh,
            RiskMetricType riskMetric
        ) {
            var xLow = riskMetric == RiskMetricType.HazardExposureRatio ? 0.01 : 1e-5;
            var xHigh = riskMetric == RiskMetricType.HazardExposureRatio ? 100000 : 100;
            var record = createRiskRatioRecord(
                boxLow,
                boxHigh,
                boxMedian,
                double.NaN,
                double.NaN,
                [2.5, 50, 97.5]
            );
            var section = new SingleRiskRatioSection() {
                LeftMargin = xLow,
                RightMargin = xHigh,
                SingleRiskRecord = record,
                Threshold = 100,
                RiskMetric = riskMetric,
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)
            };
            var chart = new SingleRiskRatioHeatMapCreator(section, false);
            RenderChart(chart, $"TestCreateNoUnc_{riskMetric}_{id}");
            AssertIsValidView(section);
        }

        [TestMethod]
        [DataRow(1, 200, 400, 1000, 150, 2000, RiskMetricType.HazardExposureRatio)]
        [DataRow(2, 200, 400, 1000, 150, double.PositiveInfinity, RiskMetricType.HazardExposureRatio)]
        [DataRow(3, 200, 400, double.PositiveInfinity, 150, double.PositiveInfinity, RiskMetricType.HazardExposureRatio)]
        [DataRow(4, 200, double.PositiveInfinity, double.PositiveInfinity, 150, double.PositiveInfinity, RiskMetricType.HazardExposureRatio)]
        [DataRow(1, 0.001, 0.01, 1, 0.0005, 3, RiskMetricType.ExposureHazardRatio)]
        [DataRow(2, 0.001, 0.01, 1, 0, 3, RiskMetricType.ExposureHazardRatio)]
        [DataRow(3, 0, 0.01, 1, 0, 3, RiskMetricType.ExposureHazardRatio)]
        [DataRow(4, 0, 0, 1, 0, 3, RiskMetricType.ExposureHazardRatio)]
        public void SingleRiskRatioHeatMapCreator_TestCreateUnc(
            int id,
            double boxLow,
            double boxMedian,
            double boxHigh,
            double wiskerLow,
            double wiskerHigh,
            RiskMetricType riskMetric
        ) {
            var xLow = riskMetric == RiskMetricType.HazardExposureRatio ? 0.01 : 1e-5;
            var xHigh = riskMetric == RiskMetricType.HazardExposureRatio ? 100000 : 100;
            var record = createRiskRatioRecord(
                boxLow,
                boxHigh,
                boxMedian,
                wiskerLow,
                wiskerHigh,
                [2.5, 50, 97.5]
            );
            var section = new SingleRiskRatioSection() {
                LeftMargin = xLow,
                RightMargin = xHigh,
                SingleRiskRecord = record,
                Threshold = 100,
                RiskMetric = riskMetric,
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)
            };
            var chart = new SingleRiskRatioHeatMapCreator(section, true);
            RenderChart(chart, $"TestCreateUnc_{riskMetric}_{id}");
            AssertIsValidView(section);
        }

        private static SubstanceRiskDistributionRecord createRiskRatioRecord(
            double boxLow,
            double boxHigh,
            double boxMedian,
            double wiskerLow,
            double wiskerHigh,
            double[] percentages
        ) {
            var percentiles = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = [boxLow, boxMedian, boxHigh],
            };
            for (int i = 0; i < 10; i++) {
                if (i == 0) {
                    // For 1 of the 10 records, add the wiskerLow and wiskeHigh
                    percentiles.AddUncertaintyValues([wiskerLow, double.NaN, wiskerHigh]);
                } else {
                    // For 9 of the 10 records, just add boxLow and boxHigh to make sure median
                    // of uncertainty values is in range of boxLow and boxHigh
                    percentiles.AddUncertaintyValues([boxLow, boxMedian, boxHigh]);
                }
            }
            var record = new SubstanceRiskDistributionRecord() {
                RiskPercentiles = percentiles,
                UncertaintyLowerLimit = 2.5,
                UncertaintyUpperLimit = 97.5
            };
            return record;
        }
    }
}
