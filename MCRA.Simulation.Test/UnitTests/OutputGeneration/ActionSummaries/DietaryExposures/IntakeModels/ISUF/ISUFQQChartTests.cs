using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ISUF
    /// </summary>
    [TestClass]
    public class ISUFQQChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ISUFQQChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();
            var e = NormalDistribution.NormalSamples(number, 0, .1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            var counter = 0;
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    Zhat = item,
                    Z = item + e[counter],
                });
                counter++;
            }
            var section = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = .5,
            };

            var chart = new ISUFQQChartCreator(section);
            RenderChart(chart, $"TestCreate");
        }
    }
}


