using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFoodCompound
    /// </summary>
    [TestClass]
    public class UpperDistributionFoodCompoundPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodCompoundPieChart_Test1() {

            var mockData = new List<DistributionFoodCompoundRecord>(){
                new DistributionFoodCompoundRecord(){CompoundName = "C1", FoodName = "Apple0", Contribution = 10},
                new DistributionFoodCompoundRecord(){CompoundName = "C2", FoodName = "Apple2", Contribution = 12},
                new DistributionFoodCompoundRecord(){CompoundName = "C3", FoodName = "Apple3", Contribution = 2},
                new DistributionFoodCompoundRecord(){CompoundName = "C4", FoodName = "Apple4", Contribution = 3},
                new DistributionFoodCompoundRecord(){CompoundName = "C5", FoodName = "Apple5", Contribution = 22},
                new DistributionFoodCompoundRecord(){CompoundName = "C6", FoodName = "Apple6", Contribution = 38},
                new DistributionFoodCompoundRecord(){CompoundName = "C7", FoodName = "Apple7", Contribution = 5},
                new DistributionFoodCompoundRecord(){CompoundName = "C8", FoodName = "Apple8", Contribution = 18},
            };
            var section = new UpperDistributionFoodCompoundSection() {
                Records = mockData,
            };

            var chart = new UpperDistributionFoodCompoundPieChartCreator(section, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
