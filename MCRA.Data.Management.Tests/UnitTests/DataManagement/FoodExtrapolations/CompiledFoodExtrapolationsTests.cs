using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledFoodExtrapolationsTests : CompiledTestsBase {
        protected Func<IDictionary<string, Food>> _getFoodsDelegate;
        protected Func<IDictionary<Food, ICollection<Food>>> _getFoodExtrapolationsDelegate;

        [TestMethod]
        public void CompiledFoodExtrapolations_TestGetAllFoodExtrapolationsMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodExtrapolationsTests\FoodExtrapolationFoods"),
                (ScopingType.FoodExtrapolations, @"FoodExtrapolationsTests\FoodExtrapolations")
            );

            var foods = _getFoodsDelegate.Invoke();
            var foodExtrapolations = _getFoodExtrapolationsDelegate.Invoke();

            Assert.AreEqual(3, foodExtrapolations.Count);

            foods.TryGetValue("A", out Food apple);
            foods.TryGetValue("P", out Food pear);
            foods.TryGetValue("O", out Food mandarins);
            foods.TryGetValue("M", out Food oranges);
            foods.TryGetValue("CF", out Food citrusFruits);

            Assert.IsTrue(foodExtrapolations.TryGetValue(pear, out var raPear) && raPear.Contains(apple));
            Assert.IsTrue(foodExtrapolations.TryGetValue(mandarins, out var raMandarins) && raMandarins.Contains(citrusFruits));
            Assert.IsTrue(foodExtrapolations.TryGetValue(oranges, out var raOranges) && raOranges.Contains(citrusFruits));
        }

        [TestMethod]
        public void CompiledFoodExtrapolations_TestGetAllFoodExtrapolationsScope() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodExtrapolations, @"FoodExtrapolationsTests\FoodExtrapolations")
            );

            var foods = _getFoodsDelegate.Invoke();
            var foodExtrapolations = _getFoodExtrapolationsDelegate.Invoke();

            Assert.AreEqual(5, foods.Count);
            Assert.IsTrue(foods.TryGetValue("A", out Food apple) && apple.Name.Equals("A"));
            Assert.IsTrue(foods.TryGetValue("P", out Food pear) && pear.Name.Equals("P"));
            Assert.IsTrue(foods.TryGetValue("M", out Food mandarins) && mandarins.Name.Equals("M"));
            Assert.IsTrue(foods.TryGetValue("O", out Food oranges) && oranges.Name.Equals("O"));
            Assert.IsTrue(foods.TryGetValue("CF", out Food citrusFruits) && citrusFruits.Name.Equals("CF"));
        }
    }
}
