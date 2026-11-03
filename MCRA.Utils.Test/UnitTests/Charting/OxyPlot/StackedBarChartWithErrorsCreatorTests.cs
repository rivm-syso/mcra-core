using MCRA.Utils.Charting.OxyPlot;

namespace MCRA.Utils.Test.UnitTests.Charting.OxyPlot {

    [TestClass]
    public class StackedBarChartWithErrorsCreatorTests : ChartCreatorTestsBase {

        [TestMethod]
        public void StackedBarChartWithErrorsCreator_TestCreateSimple() {
            var categories = new List<string>() { "Sub-category 1", "Sub-category 2", "Sub-category 3" };
            var data = new List<StackedColumnWithErrorItem>() {
                new() {
                    CategoryIndex = 0,
                    Label = "Category 1",
                    Lower = 5,
                    Upper = 7,
                    Subcategories = categories
                        .Zip([0.1, 0.4, 0.8])
                        .ToDictionary(r => r.First, r => r.Second)
                },
                new() {
                    CategoryIndex = 0,
                    Label = "Category 2",
                    Lower = 5,
                    Upper = 7,
                    Subcategories = categories
                        .Take(2)
                        .Zip([0.7, 0.6])
                        .ToDictionary(r => r.First, r => r.Second)
                },
            };
            var chart = new StackedBarChartWithErrorsCreator(data, true);
            WritePng(chart, $"TestCreate");
        }

        [DataRow(1, 3)]
        [DataRow(3, 3)]
        [DataRow(5, 4)]
        [DataRow(8, 6)]
        [TestMethod]
        public void StackedBarChartWithErrorsCreator_TestCreateScenarios(
            int numberOfCategories,
            int numberOfSubCategories
        ) {
            var allSubcategories = Enumerable.Range(1, numberOfSubCategories)
                .Select(i => $"Sub-category {i}")
                .ToList();

            var random = new Random(HashCode.Combine(numberOfCategories, numberOfSubCategories, 20261110));
            var data = new List<StackedColumnWithErrorItem>(numberOfCategories);

            for (var categoryIndex = 0; categoryIndex < numberOfCategories; categoryIndex++) {
                var lower = Math.Round(random.NextDouble() * 10, 3);
                var upper = Math.Round(lower + 0.1 + random.NextDouble() * 5, 3);

                // Ensure at least one category does not contain all sub-categories.
                var subcategoryCountForThisCategory = categoryIndex == 0 && numberOfSubCategories > 1
                    ? numberOfSubCategories - 1
                    : random.Next(1, numberOfSubCategories + 1);

                var selectedSubcategories = allSubcategories
                    .OrderBy(_ => random.Next())
                    .Take(subcategoryCountForThisCategory)
                    .ToList();

                var subcategoryValues = selectedSubcategories.ToDictionary(
                    name => name,
                    _ => Math.Round(random.NextDouble(), 3)
                );

                data.Add(new StackedColumnWithErrorItem {
                    CategoryIndex = categoryIndex,
                    Label = $"Category {categoryIndex + 1}",
                    Lower = lower,
                    Upper = upper,
                    Subcategories = subcategoryValues
                });
            }

            var chart = new StackedBarChartWithErrorsCreator(data, true);
            WritePng(chart, $"TestCreate_RandomDataRows_{numberOfCategories}_{numberOfSubCategories}");
        }
    }
}
