using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement.Concentrations {
    [TestClass]
    public class SubsetManagerConcentrationsTests : CompiledTestsBase {

        [TestMethod]
        public void SubsetManager_SelectedSamplesTestFoodSubset() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSubset"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesSubset"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests/AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests/AnalyticalMethodCompoundsSimple")
            );
            var config = Project.ModelledFoodsSettings;
            config.RestrictToModelledFoodSubset = true;
            config.ModelledFoodSubset = ["A"];

            Assert.HasCount(2, SubsetManager.SelectedFoodSamples);
            Assert.AreEqual("FS1,FS2", string.Join(",", SubsetManager.SelectedFoodSamples.Select(i => i.Code)));

            // Reset subset manager and change project settings
            ResetSubsetManager();
            config.RestrictToModelledFoodSubset = false;

            Assert.HasCount(4, SubsetManager.SelectedFoodSamples);
            Assert.AreEqual("FS1,FS2,FS3,FS4", string.Join(",", SubsetManager.SelectedFoodSamples.Select(i => i.Code)));
        }
    }
}
