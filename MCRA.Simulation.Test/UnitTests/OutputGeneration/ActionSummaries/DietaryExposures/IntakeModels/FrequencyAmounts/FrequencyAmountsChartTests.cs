using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, FrequencyAmounts
    /// </summary>
    [TestClass]
    public class FrequencyAmountsChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void FrequencyAmountsChart_Test1() {
            var frequencyAmountRelations = new List<FrequencyAmountRelationRecord> {
                new FrequencyAmountRelationRecord() {
                    NumberOfDays = 1,
                    LowerBox = 10,
                    LowerWisker = 8,
                    Median = 15,
                    UpperBox = 21,
                    UpperWisker = 30,
                },
                new FrequencyAmountRelationRecord() {
                    NumberOfDays = 2,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new FrequencyAmountRelationRecord() {
                    NumberOfDays = 3,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new FrequencyAmountRelationRecord() {
                    NumberOfDays = 4,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new FrequencyAmountRelationRecord() {
                    NumberOfDays = 5,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new FrequencyAmountRelationRecord() {
                    NumberOfDays = 8,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                }
            };
            var frequencyAmountsSummarySection = new FrequencyAmountSummarySection() {
                FrequencyAmountRelations = frequencyAmountRelations,
            };

            var chart = new FrequencyAmountChartCreator(frequencyAmountsSummarySection, "mg/kg");
            RenderChart(chart, $"TestCreate");
        }
    }
}


