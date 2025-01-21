using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Generic.Diagnostics;
using MCRA.Utils.Statistics;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {
    /// <summary>
    /// OutputGeneration, Generic, Diagnostics
    /// </summary>
    [TestClass]
    public class DiagnosticsSectionTests : SectionTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void DiagnosticsSection_ChartTest() {
            var mu = 100;
            var sigma = 30;
            var nominalSize = 10000;
            var bootstrapSize = 5000;
            var runs = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var percentages = new double[] { 50, 99 };
            var intakes = NormalDistribution.Samples(random, mu, sigma, nominalSize);
            var diagnosticsSection = new DiagnosticsSection();
            diagnosticsSection.Summarize(intakes, null, percentages, bootstrapSize);
            var ix = 1;
            if (diagnosticsSection.MCSigmas != null) {
                var chart = new DiagnosticsChartCreator(diagnosticsSection.MCSigmas, 300, 400, percentages[ix], bootstrapSize);
                chart.CreateToPng(TestUtilities.ConcatWithOutputPath($"_Diagnostics MC {percentages[ix]}.png"));
            }

            for (int ii = 0; ii < runs; ii++) {
                var bootstrapSample = new List<double>();
                for (int i = 0; i < nominalSize; i++) {
                    bootstrapSample.Add(intakes.ElementAt(random.Next(nominalSize)));
                }
                var intakesUnc = new List<double>();
                for (int i = 0; i < bootstrapSize; i++) {
                    intakesUnc.Add(bootstrapSample.ElementAt(random.Next(nominalSize)));
                }
                diagnosticsSection.SummarizeUncertainty(intakesUnc, null, percentages);
            }
            if (diagnosticsSection.MCSigmas != null) {
                var chart = new DiagnosticsChartCreator(diagnosticsSection.MCSigmas, 300, 400, percentages[ix], bootstrapSize, diagnosticsSection.BootstrapSigmas);
                chart.CreateToPng(TestUtilities.ConcatWithOutputPath($"_Diagnostics Bootstrap {percentages[ix]}.png"));
            }
            AssertIsValidView(diagnosticsSection);
        }
    }
}