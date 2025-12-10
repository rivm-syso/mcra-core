using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    /// <summary>
    /// Runs specific SubsetManager tests with regard to foods
    /// </summary>
    [TestClass]
    public class SubsetManagerFoodsTests : CompiledTestsBase {

        [TestMethod]
        public void SubsetManagerFoods_SubsetManagerMeasuredFoodsNoSamplesTest() {
            RawDataProvider.SetDataTables((ScopingType.Foods, @"FoodsTests/FoodsSimple"));

            Assert.IsEmpty(SubsetManager.AllConsumedFoods);
            Assert.IsEmpty(SubsetManager.AllModelledFoods);
            Assert.HasCount(3, SubsetManager.AllFoods);
        }

        [TestMethod]
        public void SubsetManagerFoods_SubsetManagerConsumedFoodsNoConsumptionsTest() {
            RawDataProvider.SetDataTables((ScopingType.Foods, @"FoodsTests/FoodsSimple"));

            Assert.IsEmpty(SubsetManager.AllModelledFoods);
            Assert.IsEmpty(SubsetManager.AllConsumedFoods);
            Assert.HasCount(3, SubsetManager.AllFoods);
        }

        [TestMethod]
        public void SubsetManagerFoods_SubsetManagerMeasuredFoodsFromSamplesTest() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesSimple")
            );

            Assert.HasCount(3, SubsetManager.AllModelledFoods);
            Assert.HasCount(3, SubsetManager.AllFoods);
            Assert.IsEmpty(SubsetManager.AllConsumedFoods);
            Assert.AreEqual("A,B,D", string.Join(",", SubsetManager.AllModelledFoods.Select(f => f.Code)));
        }

        /// <summary>
        /// Tests correct loading of the foods. Verification by making sure that only the
        /// expected foods are part of the foods of the compiled datasource.
        /// </summary>
        [TestMethod]
        public void FoodsDataTest_Foods() {
            RawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.Foods, SourceTableGroup.FoodTranslations, SourceTableGroup.Processing]);
            CompiledDataManager.GetAllFoodTranslations();
            CompiledDataManager.GetAllProcessingFactors();
            var foods = CompiledDataManager.GetAllFoods();
            var foodCodes = foods.Keys.ToList();

            Assert.HasCount(8, foods);
            CollectionAssert.Contains(foodCodes, "FRUITMIX");
            CollectionAssert.Contains(foodCodes, "APPLE");
            CollectionAssert.Contains(foodCodes, "APPLE$FUJI");
            CollectionAssert.Contains(foodCodes, "APPLE$ELSTAR");
            CollectionAssert.Contains(foodCodes, "APPLE_Cooked");
            CollectionAssert.Contains(foodCodes, "APPLE_Peeled");
            CollectionAssert.Contains(foodCodes, "BANANAS");
            CollectionAssert.Contains(foodCodes, "PINEAPPLE");
        }

        /// <summary>
        /// Tests correct loading of the food properties. Verification by checking the expected
        /// properties for the foods with properties (unit weight for the foods apple, bananas,
        /// and pineapple) and test for no properties on the other foods in the compiled
        /// datasource.
        /// </summary>
        [TestMethod]
        public void FoodsDataTest_FoodProperties() {
            RawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.Foods, SourceTableGroup.Processing, SourceTableGroup.FoodTranslations, SourceTableGroup.MarketShares]);
            var foods = CompiledDataManager.GetAllFoods();
            CompiledDataManager.GetAllProcessingFactors();
            CompiledDataManager.GetAllFoodTranslations();
            CompiledDataManager.GetAllMarketShares();

            var foodFruitMix = foods["FRUITMIX"];
            var foodApple = foods["APPLE"];
            var foodAppleCooked = foods["APPLE_Cooked"];
            var foodApplePeeled = foods["APPLE_Peeled"];
            var foodAppleFuji = foods["APPLE$FUJI"];
            var foodAppleElstar = foods["APPLE$ELSTAR"];
            var foodPineapple = foods["PINEAPPLE"];
            var foodBananas = foods["BANANAS"];

            Assert.IsNull(foodFruitMix.Properties);
            Assert.AreEqual(100, foodApple.Properties.UnitWeight);
            Assert.IsNull(foodAppleCooked.Properties);
            Assert.IsNull(foodApplePeeled.Properties);
            Assert.IsNull(foodAppleFuji.Properties);
            Assert.IsNull(foodAppleElstar.Properties);
            Assert.IsNull(foodBananas.Properties);
            Assert.AreEqual(300, foodPineapple.Properties.UnitWeight);

            Assert.IsNull(foodFruitMix.Properties);
            Assert.AreEqual(0.1, foodApple.MarketShare.BrandLoyalty);
            Assert.IsNull(foodAppleCooked.Properties);
            Assert.IsNull(foodApplePeeled.Properties);
            Assert.IsNull(foodAppleFuji.Properties);
            Assert.IsNull(foodAppleElstar.Properties);
            Assert.AreEqual(0.2, foodBananas.MarketShare.BrandLoyalty);
            Assert.IsNull(foodPineapple.MarketShare);
        }

        /// <summary>
        /// Tests correct loading of the food market shares. Verification by checking the
        /// that the expected market shares exist in the compiled datasource, and no other
        /// market shares exist.
        /// </summary>
        [TestMethod]
        public void FoodsDataTest_MarketShares() {
            RawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", [SourceTableGroup.Foods, SourceTableGroup.MarketShares]);
            var marketShares = CompiledDataManager.GetAllMarketShares();
            var marketSharesFoodCodes = marketShares.Select(ms => ms.Food.Code).ToList();
            Assert.HasCount(4, marketShares);
            CollectionAssert.Contains(marketSharesFoodCodes, "APPLE$FUJI");
            CollectionAssert.Contains(marketSharesFoodCodes, "APPLE$ELSTAR");
            CollectionAssert.Contains(marketSharesFoodCodes, "APPLE");
            CollectionAssert.Contains(marketSharesFoodCodes, "BANANAS");
        }

        /// <summary>
        /// Tests correct loading of the food translations. Verification by checking that
        /// the expected translations exist in the compiled data source, and no other food
        /// translations exist.
        /// </summary>
        [TestMethod]
        public void FoodsDataTest_FoodTranslations() {
            RawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", [SourceTableGroup.Foods, SourceTableGroup.FoodTranslations]);
            var foodTranslations = CompiledDataManager.GetAllFoodTranslations();
            var foods = CompiledDataManager.GetAllFoods().Values;
            Assert.HasCount(3, foodTranslations);
            var codesFoodTo = foodTranslations.Select(ft => ft.FoodTo.Code).ToList();
            CollectionAssert.Contains(codesFoodTo, "PINEAPPLE");
            CollectionAssert.Contains(codesFoodTo, "BANANAS");
            CollectionAssert.Contains(codesFoodTo, "APPLE");
        }
    }
}
