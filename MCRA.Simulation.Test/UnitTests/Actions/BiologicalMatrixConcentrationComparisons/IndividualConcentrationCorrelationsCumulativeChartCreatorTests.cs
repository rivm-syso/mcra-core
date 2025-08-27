using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.UnitTests.OutputGeneration;
using MCRA.Utils.Statistics;


namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// OutputGeneration, ActionSummaries
    /// </summary>
    [TestClass]
    public class BiologicalMatrixConcentrationComparisons_IndividualDayTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize and test IndividualConcentrationCorrelationsCumulativeChartCreator view
        /// </summary>
        [TestMethod]
        [DataRow(1, 1, "bothpos")]
        [DataRow(0, 0, "bothzero")]
        [DataRow(0, 1, "modelled")]
        [DataRow(1, 0, "monitoring")]
        public void IndividualConcentrationCorrelationsCumulativeChartCreator_ValidateChartAndHtml(
            double multiplier1,
            double multiplier2,
            string title
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new IndividualConcentrationCorrelationsCumulativeSection();
            var substance = "substance";
            var mainRecord = new DayConcentrationCorrelationsBySubstanceRecord() {
                SubstanceCode = substance,
                MonitoringVersusModelExposureRecords = []
            };
            for (var i = 0; i < 10; i++) {
                var draw = random.NextDouble();
                var record = new HbmVsModelledIndividualConcentrationRecord() {
                    Individual = i.ToString(),
                    ModelledExposure = draw * multiplier1,
                    MonitoringConcentration = 2 * draw * multiplier2,
                };
                mainRecord.MonitoringVersusModelExposureRecords.Add(record);
            }
            section.Records = [mainRecord];
            var chart = new IndividualConcentrationCorrelationCumulativeChartCreator(
                section,
                substance,
                "mg/g",
                "mg/g",
                2.5,
                97.5,
                375,
                300
            );

            RenderChart(chart, $"TestCreate_{title}");
            AssertIsValidView(section);
        }
    }
}