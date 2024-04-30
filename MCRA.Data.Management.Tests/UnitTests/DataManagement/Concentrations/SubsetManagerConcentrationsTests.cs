using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement.Concentrations {
    [TestClass]
    public class SubsetManagerConcentrationsTests : SubsetManagerTestsBase {

        [TestMethod]
        public void SubsetManager_SelectedSamplesTestFoodSubset() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSubset"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSubset"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests\AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests\AnalyticalMethodCompoundsSimple")
            );
            var config = _project.GetModuleConfiguration<ModelledFoodsModuleConfig>();
            config.RestrictToModelledFoodSubset = true;
            config.ModelledFoodSubset = new List<string>() { "A" };

            Assert.AreEqual(2, _subsetManager.SelectedFoodSamples.Count);
            Assert.AreEqual("FS1,FS2", string.Join(",", _subsetManager.SelectedFoodSamples.Select(i => i.Code)));

            // Reset subset manager and change project settings
            _subsetManager = new SubsetManager(_compiledDataManager, _project);
            config.RestrictToModelledFoodSubset = false;

            Assert.AreEqual(4, _subsetManager.SelectedFoodSamples.Count);
            Assert.AreEqual("FS1,FS2,FS3,FS4", string.Join(",", _subsetManager.SelectedFoodSamples.Select(i => i.Code)));
        }
    }
}
