using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class ConcentrationExtrapolationsSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Test ConcentrationExtrapolationsSummarySection view
        /// </summary>
        [TestMethod]
        public void ConcentrationExtrapolationsSummarySection_Test() {
            var section = new ConcentrationExtrapolationsSummarySection();
            section.Records = new List<ConcentrationExtrapolationSummaryRecord>() {
                new ConcentrationExtrapolationSummaryRecord() {
                    FoodCode ="F1",
                    FoodName = "F1",
                    ActiveSubstanceCode = "C",
                    ActiveSubstanceName = "C",
                    ExtrapolatedFoodCode = "E1",
                    ExtrapolatedFoodName = "E1",
                    MeasuredSubstanceCode = "M1",
                    MeasuredSubstanceName = "M1",
                    NumberOfMeasurements = 10,
                }
            };
            AssertIsValidView(section);
        }
    }
}
