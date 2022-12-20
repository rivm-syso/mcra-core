using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.RelativePotencyFactors {
    /// <summary>
    /// OutputGeneration, ActionSummaries, RelativePotencyFactors
    /// </summary>
    [TestClass]
    public class RelativePotencyFactorsChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart and test RelativePotencyFactorsSummarySection view
        /// </summary>
        [TestMethod]
        public void RelativePotencyFactorsChart_TestNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var mockData = new List<RelativePotencyFactorsSummaryRecord>();
            var substances = Enumerable.Range(1, 10).Select(r => $"Compound {r}").ToList();
            foreach (var substance in substances) {
                mockData.Add(new RelativePotencyFactorsSummaryRecord() {
                    CompoundCode = substance,
                    CompoundName = substance,
                    RelativePotencyFactor = random.NextDouble(),
                });
            }
            var section = new RelativePotencyFactorsSummarySection() {
                Records = mockData,
            };

            var chart = new RelativePotencyFactorsChartCreator(section);
            RenderChart(chart, $"TestRelativePotencyFactors");
        }

        /// <summary>
        /// Create chart and test view
        /// </summary>
        [TestMethod]
        public void RelativePotencyFactorsChart_TestUncertainty() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var mockData = new List<RelativePotencyFactorsSummaryRecord>();
            var substances = Enumerable.Range(1, 10).Select(r => $"Compound {r}").ToList();
            foreach (var substance in substances) {
                var uncertaintValues = new List<double>();
                for (int ii = 0; ii < 20; ii++) {
                    uncertaintValues.Add(random.NextDouble());
                }
                mockData.Add(new RelativePotencyFactorsSummaryRecord() {
                    CompoundCode = substance,
                    CompoundName = substance,
                    RelativePotencyFactor = uncertaintValues.Median(),
                    RelativePotencyFactorUncertaintyValues = uncertaintValues,
                });
            }
            var section = new RelativePotencyFactorsSummarySection() {
                Records = mockData,
            };
            AssertIsValidView(section);
            var chart = new RelativePotencyFactorsChartCreator(section);
            RenderChart(chart, $"TestRelativePotencyFactorsUnc");
        }
    }
}
