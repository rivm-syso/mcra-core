using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass]
    public class UpperDistributionFoodAsMeasuredPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredPieChart_Test1() {

            var mockData = new List<DistributionFoodRecord>(){
                new DistributionFoodRecord(){FoodName = "AppleAppleApplple40", Contribution = 10},
                new DistributionFoodRecord(){FoodName = "Apple2", Contribution = 12},
                new DistributionFoodRecord(){FoodName = "Apple3", Contribution = 32},
                new DistributionFoodRecord(){FoodName = "Apple4", Contribution = 3},
                new DistributionFoodRecord(){FoodName = "Apple5", Contribution = 22},
                new DistributionFoodRecord(){FoodName = "Apple6", Contribution = 8},
                new DistributionFoodRecord(){FoodName = "Apple7", Contribution = 5},
                new DistributionFoodRecord(){FoodName = "Apple8", Contribution = 8},
            };
            var section = new UpperDistributionFoodAsMeasuredSection() {
                Records = mockData,
            };

            var chart = new UpperDistributionFoodAsMeasuredPieChartCreator(section, section.Records, false);
            RenderChart(chart, $"TestCreate");
        }
    }
}
