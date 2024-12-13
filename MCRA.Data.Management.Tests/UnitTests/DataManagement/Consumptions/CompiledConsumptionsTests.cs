using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledConsumptionsTests : CompiledTestsBase {

        protected Func<IDictionary<string, FoodSurvey>> _getFoodSurveysDelegate;
        protected Func<IDictionary<string, Individual>> _getIndividualsDelegate;
        protected Func<IDictionary<string, Food>> _getFoodsDelegate;
        protected Func<IList<FoodConsumption>> _getFoodConsumptionsDelegate;
        protected Func<IList<IndividualDay>> _getIndividualDayDelegate;

        [TestMethod]
        public void CompiledConsumptions_TestIndividualsOnly() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );

            var surveys = _getFoodSurveysDelegate.Invoke();
            var individuals = _getIndividualsDelegate.Invoke();

            Assert.AreEqual(3, surveys.Count);
            Assert.AreEqual(5, individuals.Count);

            CollectionAssert.AreEquivalent(new[] { "S1", "S2", "S3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
        }

        [TestMethod]
        public void CompiledConsumptions_TestIndividualsOnlyWithSurveyScope() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, ["s2"]);

            var surveys = _getFoodSurveysDelegate.Invoke();
            var individuals = _getIndividualsDelegate.Invoke();

            Assert.AreEqual(1, surveys.Count);
            Assert.AreEqual(2, individuals.Count);

            CollectionAssert.AreEquivalent(new[] { "S2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
        }

        [TestMethod]
        public void CompiledConsumptions_TestConsumptionsOnly() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions")
            );

            var surveys = _getFoodSurveysDelegate.Invoke();
            var individuals = _getIndividualsDelegate.Invoke();
            var consumptions = _getFoodConsumptionsDelegate.Invoke();

            Assert.AreEqual(0, surveys.Count);
            Assert.AreEqual(0, individuals.Count);
            Assert.AreEqual(0, consumptions.Count);
        }

        [TestMethod]
        public void CompiledConsumptions_TestWithIndividuals() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );

            var surveys = _getFoodSurveysDelegate.Invoke();
            var individuals = _getIndividualsDelegate.Invoke();
            var consumptions = _getFoodConsumptionsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, surveys.Count);
            Assert.AreEqual(5, individuals.Count);
            Assert.AreEqual(4, foods.Count);
            Assert.AreEqual(14, consumptions.Count);

            CollectionAssert.AreEquivalent(new[] { "S1", "S2", "S3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, consumptions.Select(c => c.Individual.Id).Distinct().ToList());
        }

        [TestMethod]
        public void CompiledConsumptions_TestIndividualsSurvey() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );

            var surveys = _getFoodSurveysDelegate.Invoke();
            var individuals = _getIndividualsDelegate.Invoke();
            var consumptions = _getFoodConsumptionsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, surveys.Count);
            Assert.AreEqual(5, individuals.Count);
            Assert.AreEqual(4, foods.Count);
            Assert.AreEqual(14, consumptions.Count);

            CollectionAssert.AreEquivalent(new[] { "S1", "S2", "S3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, consumptions.Select(c => c.Individual.Id).Distinct().ToList());
        }

        [TestMethod]
        public void CompiledConsumptions_TestIndividualsSurveyFilterFoods() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2", "f3"]);
            var surveys = _getFoodSurveysDelegate.Invoke();
            var individuals = _getIndividualsDelegate.Invoke();
            var consumptions = _getFoodConsumptionsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, surveys.Count);
            Assert.AreEqual(5, individuals.Count);
            Assert.AreEqual(2, foods.Count);
            Assert.AreEqual(6, consumptions.Count);

            CollectionAssert.AreEquivalent(new[] { "S1", "S2", "S3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "f2", "f3" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { 3, 4, 5 }, consumptions.Select(c => c.Individual.Id).Distinct().ToList());
        }

        /// <summary>
        /// Tests correct loading of FoodEx2 coded foods.
        /// Moved here from MCRA.Test
        /// </summary>
        [TestMethod]
        public void CompiledConsumptions_TestFoodEx2Data() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodEx2Survey"),
                (ScopingType.Consumptions, @"ConsumptionsTests\FoodEx2Consumptions"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\FoodEx2Individuals")
            );

            _getFoodConsumptionsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke().Values.ToList();
            var foodWithFacets = foods.First(f => f.Code == "A01CE#F20.A07QF");
            var foodCodes = foods.Select(f => f.Code).ToList();
            CollectionAssert.Contains(foodCodes, "A01CE#F20.A07QF");
            CollectionAssert.Contains(foodCodes, "A01CE");
        }

        [TestMethod]
        public void CompiledConsumptions_TestFoodEx2FoodHierarchy() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodEx2Survey"),
                (ScopingType.Consumptions, @"ConsumptionsTests\FoodEx2Consumptions"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\FoodEx2Individuals")
            );

            _getFoodConsumptionsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke().Values.ToList();
            var orderedFoods = TreeOrder(foods).ToList();
        }

        private static IEnumerable<Food> TreeOrder(IEnumerable<Food> nodes) {
            var root = nodes.First(node => node.Parent == null);
            var childrenLookup = nodes
                .Where(node => node.Parent != null)
                .ToLookup(node => node.Parent.Code);
            return TreeOrder(root, childrenLookup);
        }

        private static IEnumerable<Food> TreeOrder(Food root, ILookup<string, Food> childrenLookup) {
            yield return root;
            if (!childrenLookup.Contains(root.Code)) {
                yield break;
            }
            foreach (var child in childrenLookup[root.Code]) {
                foreach (var node in TreeOrder(child, childrenLookup)) {
                    yield return node;
                }
            }
        }
    }
}
