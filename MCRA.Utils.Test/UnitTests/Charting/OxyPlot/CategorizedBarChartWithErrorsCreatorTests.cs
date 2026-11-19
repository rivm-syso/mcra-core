using DocumentFormat.OpenXml.Drawing;
using MCRA.Utils.Charting.OxyPlot;

namespace MCRA.Utils.Test.UnitTests.Charting.OxyPlot {

    [TestClass]
    public class CategorizedBarChartWithErrorsCreatorTests : ChartCreatorTestsBase {

        [TestMethod]
        public void CategorizedBarChartWithErrorsCreator_TestCreateSimple() {
            var categories = new List<string>() { "Sub-category 1", "Sub-category 2", "Sub-category 3" };
            var data = new List<CategorizedColumnWithErrorItem>() {
                new() {
                    CategoryIndex = 0,
                    Label = "Category 1",
                    Lower = 5,
                    Upper = 7,
                    Value = 6,
                    Subcategories = categories
                        .Zip([0.1, 0.4, 0.5])
                        .ToDictionary(r => r.First, r => r.Second)
                },
                new() {
                    CategoryIndex = 0,
                    Label = "Category 2",
                    Lower = 5,
                    Upper = 7,
                    Value = 6,
                    Subcategories = categories
                        .Take(2)
                        .Zip([0.7, 0.3])
                        .ToDictionary(r => r.First, r => r.Second)
                },
            };
            var chart = new CategorizedBarChartWithErrorsCreator(data, true);
            WritePng(chart, $"TestCreate");
        }

        [DataRow(1, 3)]
        [DataRow(3, 3)]
        [DataRow(5, 4)]
        [DataRow(8, 6)]
        [TestMethod]
        public void CategorizedBarChartWithErrorsCreator_TestCreateScenarios(
            int numberOfCategories,
            int numberOfSubCategories
        ) {
            var allSubcategories = Enumerable.Range(1, numberOfSubCategories)
                .Select(i => $"Sub-category {i}")
                .ToList();

            var random = new Random(HashCode.Combine(numberOfCategories, numberOfSubCategories, 20261110));
            var data = new List<CategorizedColumnWithErrorItem>(numberOfCategories);

            for (var categoryIndex = 0; categoryIndex < numberOfCategories; categoryIndex++) {
                var lower = Math.Round(random.NextDouble() * 10, 3);
                var upper = Math.Round(lower + 0.1 + random.NextDouble() * 5, 3);
                var value = lower + (upper - lower) / 2;

                // Ensure at least one category does not contain all sub-categories.
                var subcategoryCountForThisCategory = categoryIndex == 0 && numberOfSubCategories > 1
                    ? numberOfSubCategories - 1
                    : random.Next(1, numberOfSubCategories + 1);

                var selectedSubcategories = allSubcategories
                    .OrderBy(_ => random.Next())
                    .Take(subcategoryCountForThisCategory)
                    .ToList();

                var names = selectedSubcategories.ToList();
                var raw = names.Select(_ => random.NextDouble()).ToList();
                var total = raw.Sum();
                var scaled = raw.Select(r => Math.Round(r / total, 3)).ToList();

                var subcategoryValues = names
                    .Zip(scaled, (name, value) => (name, value))
                    .ToDictionary(r => r.name, r => r.value);

                data.Add(new CategorizedColumnWithErrorItem {
                    CategoryIndex = categoryIndex,
                    Label = $"Category {categoryIndex + 1}",
                    Value = value,
                    Lower = lower,
                    Upper = upper,
                    Subcategories = subcategoryValues
                });
            }

            var chart = new CategorizedBarChartWithErrorsCreator(data, true);
            WritePng(chart, $"TestCreate_RandomDataRows_{numberOfCategories}_{numberOfSubCategories}");
        }
    }
}
