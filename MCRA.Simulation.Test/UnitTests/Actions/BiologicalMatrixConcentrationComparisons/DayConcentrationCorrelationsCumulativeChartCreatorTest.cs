﻿using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.UnitTests.OutputGeneration;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions.BiologicalMatrixConcentrationComparisons {
    /// <summary>
    /// OutputGeneration, ActionSummaries
    /// </summary>
    [TestClass]
    public class BiologicalMatrixConcentrationComparisons_IndividualTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize and ConcentrationCorrelationsCumulativeChartCreator view
        /// </summary>
        [TestMethod]
        [DataRow(1, 1, "bothpos")]
        [DataRow(0, 0, "bothzero")]
        [DataRow(0, 1, "modelled")]
        [DataRow(1, 0, "monitoring")]
        public void DayConcentrationCorrelationsCumulativeChartCreator_ValidateChartAndHtml(
            double multiplier1,
            double multiplier2,
            string title
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new DayConcentrationCorrelationsCumulativeSection();
            var substance = "substance";
            var mainRecord = new IndividualConcentrationCorrelationsBySubstanceRecord() {
                SubstanceCode = substance,
                MonitoringVersusModelExposureRecords = []
            };
            for (var i = 0; i < 10; i++) {
                var draw = random.NextDouble();
                var record = new HbmVsModelledIndividualDayConcentrationRecord() {
                    Individual = i.ToString(),
                    Day = "1",
                    ModelledExposure = draw * multiplier1,
                    MonitoringConcentration = 2 * draw * multiplier2,
                };
                mainRecord.MonitoringVersusModelExposureRecords.Add(record);
            }
            section.Records = [mainRecord];
            var chart = new DayConcentrationCorrelationsCumulativeChartCreator(
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