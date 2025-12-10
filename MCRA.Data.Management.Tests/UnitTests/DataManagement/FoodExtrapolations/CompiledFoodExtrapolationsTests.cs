using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledFoodExtrapolationsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFoodExtrapolations_TestGetAllFoodExtrapolationsMatched(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodExtrapolationsTests/FoodExtrapolationFoods"),
                (ScopingType.FoodExtrapolations, @"FoodExtrapolationsTests/FoodExtrapolations")
            );

            var foods = GetAllFoods(managerType);
            var foodExtrapolations = GetAllFoodExtrapolations(managerType);

            Assert.HasCount(3, foodExtrapolations);

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFoodExtrapolations_TestGetAllFoodExtrapolationsScope(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodExtrapolations, @"FoodExtrapolationsTests/FoodExtrapolations")
            );

            var foods = GetAllFoods(managerType);
            var foodExtrapolations = GetAllFoodExtrapolations(managerType);

            Assert.HasCount(5, foods);
            Assert.IsTrue(foods.TryGetValue("A", out Food apple) && apple.Name.Equals("A"));
            Assert.IsTrue(foods.TryGetValue("P", out Food pear) && pear.Name.Equals("P"));
            Assert.IsTrue(foods.TryGetValue("M", out Food mandarins) && mandarins.Name.Equals("M"));
            Assert.IsTrue(foods.TryGetValue("O", out Food oranges) && oranges.Name.Equals("O"));
            Assert.IsTrue(foods.TryGetValue("CF", out Food citrusFruits) && citrusFruits.Name.Equals("CF"));
        }
    }
}
