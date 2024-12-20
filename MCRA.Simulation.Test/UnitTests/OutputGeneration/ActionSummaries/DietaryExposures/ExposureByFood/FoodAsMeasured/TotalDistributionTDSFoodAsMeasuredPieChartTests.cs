﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass]
    public class TotalDistributionTDSFoodAsMeasuredPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart and test TotalDistributionTDSFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionTDSFoodAsMeasuredPieChart_Test1() {

            var mockData = new List<TDSReadAcrossFoodRecord>(){
                new TDSReadAcrossFoodRecord(){FoodName = "AppleAppleApplple40", Contribution = 10, Contributions = []},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple2", Contribution = 12, Contributions = []},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple3", Contribution = 32, Contributions = []},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple4", Contribution = 3, Contributions = []},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple5", Contribution = 22, Contributions = []},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple6", Contribution = 8, Contributions = []},
                new TDSReadAcrossFoodRecord(){FoodName = "Apple7", Contribution = 5, Contributions = []},
                new TDSReadAcrossFoodRecord(){
                    FoodName = "Apple8",
                    Contribution = 8,
                    FoodCode = "code",
                    TDSFoodCode = "TDS",
                    TDSFoodName = "TDS",
                    Translation = "ra",
                    Contributions = [],
                },
            };
            var section = new TotalDistributionTDSFoodAsMeasuredSection() {
                Records = mockData,
            };

            var chart = new TotalDistributionTDSFoodAsMeasuredPieChartCreator(section);
            RenderChart(chart, $"TestCreate");
        }
    }
}
