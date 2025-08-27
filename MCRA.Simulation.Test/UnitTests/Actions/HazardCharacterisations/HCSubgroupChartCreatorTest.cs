using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.UnitTests.OutputGeneration;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions.HazardCharacterisations {
    /// <summary>
    /// OutputGeneration, ActionSummaries
    /// </summary>
    [TestClass]
    public class HazardCharacterisations_SubgroupTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize and ConcentrationCorrelationsCumulativeChartCreator view
        /// </summary>
        [TestMethod]
        public void HazardCharSubgroupChartCreator_ValidateChartAndHtml() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var records = new List<HCSubgroupPlotRecord>();
            for (var i = 0; i < 10; i++) {
                var record = new HCSubgroupPlotRecord() {
                    Age = i + 1,
                    HazardCharacterisationValue = i * 2,
                    UncertaintyValues = [i * 2 * random.NextDouble(.5, 1.5), i * 2 * random.NextDouble(.3, 1.7)],
                };
                records.Add(record);
            }
            var section = new HazardCharacterisationsFromDataSummarySection();
            var plotRecords = new HCSubgroupSubstancePlotRecords() {
                PlotRecords = records,
            };
            var chart = new HCSubgroupChartCreator(
                section,
                plotRecords
            );

            RenderChart(chart, $"TestCreate_Subgroups");
            AssertIsValidView(section);
        }
    }
}