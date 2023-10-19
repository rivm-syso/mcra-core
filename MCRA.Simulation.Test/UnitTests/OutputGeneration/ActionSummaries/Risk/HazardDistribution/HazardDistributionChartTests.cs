using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class HazardDistributionChartTests : ChartCreatorTestBase {
        private static int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .5, 1.5).ToList();
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
            return logData.MakeHistogramBins(weights, numberOfBins, logData.Min(), logData.Max());
        }

        /// <summary>
        /// Create charts and test HazardDistributionSection view without uncertainty
        /// </summary>
        [TestMethod]
        public void HazardDistributionChart_TestWithoutUncertainty() {
            var section = new HazardDistributionSection() {
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                CEDDistributionBins = simulateBins(),
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, false),
            };

            var chart = new HazardDistributionChartCreator(section, "mg/kg ");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
        /// <summary>
        ///  Creates charts and test HazardDistributionSection view with uncertainty
        /// </summary>
        [TestMethod]
        public void HazardDistributionChart_TestWithUncertainty() {
            var section = new HazardDistributionSection() {
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                CEDDistributionBins = simulateBins(),
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, true),
            };

            var chart = new HazardDistributionChartCreator(section, "mg/kg ");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}