using MCRA.Simulation.OutputGeneration;

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
                new() {
                    NumberOfDays = 1,
                    LowerBox = 10,
                    LowerWisker = 8,
                    Median = 15,
                    UpperBox = 21,
                    UpperWisker = 30,
                },
                new() {
                    NumberOfDays = 2,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new() {
                    NumberOfDays = 3,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new() {
                    NumberOfDays = 4,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new() {
                    NumberOfDays = 5,
                    LowerBox = 11,
                    LowerWisker = 9,
                    Median = 14,
                    UpperBox = 25,
                    UpperWisker = 40,
                },
                new() {
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


