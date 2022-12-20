using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, Estimates
    /// </summary>
    [TestClass]
    public class EmpiricalResidualsQQChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void EmpiricalResidualsQQChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();
            var normalAmountsModelResidualSection = new NormalAmountsModelResidualSection() {
                Residuals = error,
            };

            var chart = new EmpiricalResidualsQQChartCreator(normalAmountsModelResidualSection);
            RenderChart(chart, $"TestCreate");
        }
    }
}


