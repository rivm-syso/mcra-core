using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass]
    public class TotalDistributionFoodAsMeasuredPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create charts and test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredPieChart_TestCreate() {
            var configs = new int[] { 0, 1, 2, 5, 10, 20 };
            for (int i = 0; i < configs.Length; i++) {
                var n = configs[i];
                var random = new McraRandomGenerator(n);
                var foods = MockFoodsGenerator.Create(n);
                var rnds = foods.Select(r => random.NextDouble()).ToList();
                var section = new TotalDistributionFoodAsMeasuredSection() {
                    Records = foods
                        .Select((r, ix) => new DistributionFoodRecord() {
                            FoodName = r.Name,
                            Contribution = (rnds[ix] / rnds.Sum()) * 100,
                            Contributions = new List<double>(),
                        })
                        .ToList()
                };

                var chart = new TotalDistributionFoodAsMeasuredPieChartCreator(section, section.Records, false);
                RenderChart(chart, $"TestCreate1");
                if (i > 0) {
                    AssertIsValidView(section);
                }
            }
        }

        /// <summary>
        /// Create charts and test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredPieChart_TestCreateNaN() {
            var mockData = new List<DistributionFoodRecord>(){
                new DistributionFoodRecord(){FoodName = "Apple", Contribution = double.NaN},
                new DistributionFoodRecord(){FoodName = "Pear", Contribution = double.NaN},
                new DistributionFoodRecord(){FoodName = "Orange", Contribution = 3},
            };
            var section = new TotalDistributionFoodAsMeasuredSection() {
                Records = mockData,
            };

            var chart = new TotalDistributionFoodAsMeasuredPieChartCreator(section, section.Records, false);
            RenderChart(chart, $"TestCreate2");
        }
    }
}
