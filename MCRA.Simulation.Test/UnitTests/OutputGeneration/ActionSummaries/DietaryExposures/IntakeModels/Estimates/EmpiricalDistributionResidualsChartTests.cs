using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, Estimates
    /// </summary>
    [TestClass]
    public class EmpiricalDistributionResidualsChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart, test NormalAmountsModelResidualSection view
        /// </summary>
        [TestMethod]
        public void EmpiricalDistributionResidualsChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();
            var section = new NormalAmountsModelResidualSection() {
                Residuals = error,
            };

            var chart = new EmpiricalDistributionResidualsChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}


