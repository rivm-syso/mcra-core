using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement.FocalFoods {

    [TestClass]
    public class SubsetManagerFocalFoodConcentrationsTests : CompiledTestsBase {

        [TestMethod]
        public void SubsetManager_TestSelectedFocalCommoditySamples1() {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSubset"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );

            var config = Project.ConcentrationsSettings;
            config.FocalCommodity = true;
            config.FocalFoods = [new() { CodeFood = "A" }];

            Assert.HasCount(2, SubsetManager.SelectedFocalCommoditySamples);
            Assert.AreEqual("FS1,FS2", string.Join(",", SubsetManager.SelectedFocalCommoditySamples.Select(i => i.Code)));

            Assert.HasCount(1, SubsetManager.SelectedFocalCommodityFoods);
            Assert.AreEqual("A", string.Join(",", SubsetManager.SelectedFocalCommodityFoods.Select(i => i.Code)));
        }

        [TestMethod]
        public void SubsetManager_TestSelectedFocalCommoditySamples2() {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSubset"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );
            var config = Project.ConcentrationsSettings;
            config.FocalCommodity = true;
            config.FocalFoods = [new() { CodeFood = "B" }, new() { CodeFood = "D" }];

            Assert.HasCount(2, SubsetManager.SelectedFocalCommoditySamples);
            Assert.AreEqual("FS3,FS4", string.Join(",", SubsetManager.SelectedFocalCommoditySamples.Select(i => i.Code)));

            Assert.HasCount(2, SubsetManager.SelectedFocalCommodityFoods);
            Assert.AreEqual("B,D", string.Join(",", SubsetManager.SelectedFocalCommodityFoods.Select(i => i.Code)));
        }
    }
}
