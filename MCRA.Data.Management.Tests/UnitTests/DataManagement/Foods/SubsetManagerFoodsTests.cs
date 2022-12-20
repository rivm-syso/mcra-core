using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    /// <summary>
    /// Runs specific SubsetManager tests with regard to foods
    /// </summary>
    [TestClass]
    public class SubsetManagerFoodsTests : SubsetManagerTestsBase {

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
        }

        [TestMethod]
        public void SubsetManagerFoods_SubsetManagerMeasuredFoodsNoSamplesTest() {
            _rawDataProvider.SetDataTables((ScopingType.Foods, @"FoodsTests\FoodsSimple"));

            Assert.AreEqual(0, _subsetManager.AllConsumedFoods.Count);
            Assert.AreEqual(0, _subsetManager.AllModelledFoods.Count);
            Assert.AreEqual(3, _subsetManager.AllFoods.Count);
        }

        [TestMethod]
        public void SubsetManagerFoods_SubsetManagerConsumedFoodsNoConsumptionsTest() {
            _rawDataProvider.SetDataTables((ScopingType.Foods, @"FoodsTests\FoodsSimple"));

            Assert.AreEqual(0, _subsetManager.AllModelledFoods.Count);
            Assert.AreEqual(0, _subsetManager.AllConsumedFoods.Count);
            Assert.AreEqual(3, _subsetManager.AllFoods.Count);
        }

        [TestMethod]
        public void SubsetManagerFoods_SubsetManagerMeasuredFoodsFromSamplesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSimple")
            );

            Assert.AreEqual(3, _subsetManager.AllModelledFoods.Count);
            Assert.AreEqual(3, _subsetManager.AllFoods.Count);
            Assert.AreEqual(0, _subsetManager.AllConsumedFoods.Count);
            Assert.AreEqual("A,B,D", string.Join(",", _subsetManager.AllModelledFoods.Select(f => f.Code)));
        }

        /// <summary>
        /// Tests correct loading of the foods. Verification by making sure that only the
        /// expected foods are part of the foods of the compiled datasource.
        /// </summary>
        [TestMethod]
        public void FoodsDataTest_Foods() {
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                new[] { SourceTableGroup.Foods, SourceTableGroup.FoodTranslations, SourceTableGroup.Processing });
            _compiledDataManager.GetAllFoodTranslations();
            _compiledDataManager.GetAllProcessingFactors();
            var foods = _compiledDataManager.GetAllFoods();
            var foodCodes = foods.Keys.ToList();

            Assert.AreEqual(8, foods.Count);
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
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                new[] { SourceTableGroup.Foods, SourceTableGroup.Processing, SourceTableGroup.FoodTranslations, SourceTableGroup.MarketShares });
            var foods = _compiledDataManager.GetAllFoods();
            _compiledDataManager.GetAllProcessingFactors();
            _compiledDataManager.GetAllFoodTranslations();
            _compiledDataManager.GetAllMarketShares();

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
            _rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", new[] { SourceTableGroup.Foods, SourceTableGroup.MarketShares });
            var marketShares = _compiledDataManager.GetAllMarketShares();
            var marketSharesFoodCodes = marketShares.Select(ms => ms.Food.Code).ToList();
            Assert.AreEqual(4, marketShares.Count);
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
            _rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", new[] { SourceTableGroup.Foods, SourceTableGroup.FoodTranslations });
            var foodTranslations = _compiledDataManager.GetAllFoodTranslations();
            var foods = _compiledDataManager.GetAllFoods().Values;
            Assert.AreEqual(3, foodTranslations.Count);
            var codesFoodTo = foodTranslations.Select(ft => ft.FoodTo.Code).ToList();
            CollectionAssert.Contains(codesFoodTo, "PINEAPPLE");
            CollectionAssert.Contains(codesFoodTo, "BANANAS");
            CollectionAssert.Contains(codesFoodTo, "APPLE");
        }
    }
}
