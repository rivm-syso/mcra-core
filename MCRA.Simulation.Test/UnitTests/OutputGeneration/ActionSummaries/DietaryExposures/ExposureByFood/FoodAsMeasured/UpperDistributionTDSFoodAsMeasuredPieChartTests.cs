using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass]
    public class UpperDistributionTDSFoodAsMeasuredPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void UpperDistributionTDSFoodAsMeasuredPieChart_Test1() {
            var mockData = new List<TDSReadAcrossFoodRecord>(){
                new TDSReadAcrossFoodRecord(){FoodName = "AppleAppleApplple40", Contribution = 10},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple2", Contribution = 12},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple3", Contribution = 32},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple4", Contribution = 3},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple5", Contribution = 22},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple6", Contribution = 8},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple7", Contribution = 5},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple8", Contribution = 8},
            };
            var section = new UpperDistributionTDSFoodAsMeasuredSection() {
                UpperDistributionTDSFoodAsMeasuredRecords = mockData,
            };
            var chart = new UpperDistributionTDSFoodAsMeasuredPieChartCreator(section);
            RenderChart(chart, $"TestCreate");
        }
    }
}
