using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement.FocalFoods {

    [TestClass]
    public class SubsetManagerFocalFoodConcentrationsTests : SubsetManagerTestsBase {

        [TestMethod]
        public void SubsetManager_TestSelectedFocalCommoditySamples1() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesSubset"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests\AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests\AnalyticalMethodCompoundsSimple")
            );

            _project.AssessmentSettings.FocalCommodity = true;
            _project.FocalFoods = new List<FocalFoodDto>() { new FocalFoodDto() { CodeFood = "A" } };

            Assert.AreEqual(2, _subsetManager.SelectedFocalCommoditySamples.Count);
            Assert.AreEqual("FS1,FS2", string.Join(",", _subsetManager.SelectedFocalCommoditySamples.Select(i => i.Code)));

            Assert.AreEqual(1, _subsetManager.SelectedFocalCommodityFoods.Count);
            Assert.AreEqual("A", string.Join(",", _subsetManager.SelectedFocalCommodityFoods.Select(i => i.Code)));
        }

        [TestMethod]
        public void SubsetManager_TestSelectedFocalCommoditySamples2() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesSubset"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests\AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests\AnalyticalMethodCompoundsSimple")
            );

            _project.AssessmentSettings.FocalCommodity = true;
            _project.FocalFoods = new List<FocalFoodDto>() { new FocalFoodDto() { CodeFood = "B" }, new FocalFoodDto() { CodeFood = "D" } };

            Assert.AreEqual(2, _subsetManager.SelectedFocalCommoditySamples.Count);
            Assert.AreEqual("FS3,FS4", string.Join(",", _subsetManager.SelectedFocalCommoditySamples.Select(i => i.Code)));

            Assert.AreEqual(2, _subsetManager.SelectedFocalCommodityFoods.Count);
            Assert.AreEqual("B,D", string.Join(",", _subsetManager.SelectedFocalCommodityFoods.Select(i => i.Code)));
        }
    }
}
