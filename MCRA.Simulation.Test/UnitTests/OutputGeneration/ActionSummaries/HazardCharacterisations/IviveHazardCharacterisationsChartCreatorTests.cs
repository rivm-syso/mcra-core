using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HazardCharacterisations {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HazardCharacterisations
    /// </summary>
    [TestClass]
    public class IviveHazardCharacterisationsChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart Ivive hazard characterisation and test IviveHazardCharacterisationsSummarySection view
        /// </summary>
        [TestMethod]
        public void IviveTargetDosesChartCreator_TestCreate() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = Enumerable.Range(1, 10).Select(r => $"Compound {r}").ToList();
            var records = substances
                .Select((r, ix) => new IviveHazardCharacterisationsSummaryRecord() {
                    CompoundCode = r,
                    CompoundName = r,
                    HazardCharacterisation = LogNormalDistribution.Draw(random, 5, 2)
                })
                .ToList();

            var section = new IviveHazardCharacterisationsSummarySection() {
                Records = records,
            };

            var chart = new IviveHazardCharacterisationsChartCreator(section, "unit");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
