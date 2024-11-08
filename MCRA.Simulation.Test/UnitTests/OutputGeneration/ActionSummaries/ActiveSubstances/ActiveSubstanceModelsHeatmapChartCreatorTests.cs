using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ActiveSubstances {

    /// <summary>
    /// OutputGeneration, ActionSummaries, ActiveSubstances
    /// </summary>
    [TestClass]
    public class ActiveSubstanceModelsHeatmapChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Test creation of assessment group memberships heatmap charts
        /// for various numbers of substances. Validates whether the chart
        /// was created successfully.
        /// </summary>
        [TestMethod]
        public void ActiveSubstanceModelsHeatmapChartCreator_TestCreate() {
            var substanceConfigs = new int[] { 0, 5, 10, 100, 1000 };
            var modelConfigs = new int[] { 2, 5, 10, 20 };
            foreach (var n in substanceConfigs) {
                foreach (var m in modelConfigs) {
                    var random = new McraRandomGenerator(m * n);
                    var substances = Enumerable.Range(1, n).Select(r => $"Compound {r}").ToList();
                    var pNaN = .1;
                    var pMissing = 0.1;
                    var models = Enumerable.Range(1, m)
                        .Select(r => new ActiveSubstanceModelRecord() {
                            Name = $"Model_{r}",
                            Code = $"Model_{r}",
                            MembershipProbabilities = substances
                                .Select(s => new ActiveSubstanceRecord() {
                                    SubstanceCode = s,
                                    SubstanceName = s,
                                    Probability = random.NextDouble() > pNaN ?
                                        DiscreteUniformDistribution.Draw(random, 0, 1)
                                        : double.NaN
                                })
                                .Where(s => random.NextDouble() > pMissing) // Randomly remove some records
                                .ToList()
                        })
                        .ToList();
                    var section = new ActiveSubstancesSummarySection() {
                        Records = models,
                        SubstanceCodes = substances,
                        SubstanceNames = substances
                    };

                    var chart = new ActiveSubstanceModelsHeatmapChartCreator(section);
                    RenderChart(chart, $"TestCreate_({n}-{m})");
                }
            }
        }
    }
}
