using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ISUF
    /// </summary>
    [TestClass]
    public class ISUFDiscreteFrequencyDistributionChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart, test ISUFModelResultsSection view 
        /// </summary>
        [TestMethod]
        public void ISUFDiscreteFrequencyDistributionChart_Test1() {
            var discreteFrequencies = new List<double>();
            var zeros = Enumerable.Repeat(0D, 20).ToList();
            discreteFrequencies.AddRange(zeros);
            discreteFrequencies[0] = .2;
            discreteFrequencies[18] = .1;
            discreteFrequencies[19] = .31;
            discreteFrequencies[17] = .51;

            var section = new ISUFModelResultsSection() {
                DiscreteFrequencies = discreteFrequencies,
                ISUFDiagnostics = [],
            };

            var chart = new ISUFDiscreteFrequencyDistributionChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}


