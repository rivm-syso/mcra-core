using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledFoodTranslationsTests : CompiledTestsBase {
        private void assertCompiledFoodsGetAllFoodTranslationsHierarchy(
            Food pie,
            Food apple,
            Food flour,
            Food wheat,
            ILookup<Food, FoodTranslation> translations
        ) {
            Assert.AreEqual(2, translations[pie].Count());
            Assert.AreEqual(60, translations[pie].Single(p => p.FoodTo == apple).Proportion);
            Assert.AreEqual(40, translations[pie].Single(p => p.FoodTo == flour).Proportion);

            Assert.AreEqual(0, translations[apple].Count());

            Assert.AreEqual(1, translations[flour].Count());
            Assert.AreEqual(wheat, translations[flour].First().FoodTo);
            Assert.AreEqual(80, translations[flour].First().Proportion);

            Assert.AreEqual(0, translations[wheat].Count());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFoodsGetAllFoodTranslationsMatchedTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodTranslationTests/FoodTranslationFoods"),
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslations")
            );

            var foods = GetAllFoods(managerType);
            var foodRecipes = GetAllFoodTranslations(managerType);

            Assert.HasCount(4, foods);
            Assert.IsTrue(foods.TryGetValue("A", out Food apple) && apple.Name.Equals("Apple"));
            Assert.IsTrue(foods.TryGetValue("F", out Food flour) && flour.Name.Equals("Flour"));
            Assert.IsTrue(foods.TryGetValue("W", out Food wheat) && wheat.Name.Equals("Wheat"));
            Assert.IsTrue(foods.TryGetValue("AP", out Food pie) && pie.Name.Equals("Apple Pie"));

            assertCompiledFoodsGetAllFoodTranslationsHierarchy(pie, apple, flour, wheat, foodRecipes.ToLookup(r => r.FoodFrom));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFoodsGetAllFoodTranslationsScopeTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslations")
            );

            var foods = GetAllFoods(managerType);
            var foodRecipes = GetAllFoodTranslations(managerType);

            Assert.HasCount(4, foods);
            Assert.IsTrue(foods.TryGetValue("A", out Food apple) && apple.Name.Equals("A"));
            Assert.IsTrue(foods.TryGetValue("F", out Food flour) && flour.Name.Equals("F"));
            Assert.IsTrue(foods.TryGetValue("W", out Food wheat) && wheat.Name.Equals("W"));
            Assert.IsTrue(foods.TryGetValue("AP", out Food pie) && pie.Name.Equals("AP"));

            assertCompiledFoodsGetAllFoodTranslationsHierarchy(pie, apple, flour, wheat, foodRecipes.ToLookup(r => r.FoodFrom));
        }
    }
}
