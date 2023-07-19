using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerConcentrationsTests : CompiledConcentrationsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getAnalyticalMethodsDelegate = () => _compiledDataManager.GetAllAnalyticalMethods();
            _getFoodsDelegate = () => _compiledDataManager.GetAllFoods();
            _getFoodSamplesDelegate = () => _compiledDataManager.GetAllFoodSamples();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerConcentrationsTests : CompiledConcentrationsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _isSubSetManagerTest = true;
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getAnalyticalMethodsDelegate = () => _subsetManager.AllAnalyticalMethods;
            _getFoodsDelegate = () => _subsetManager.AllFoodsByCode;
            _getFoodSamplesDelegate = () => _subsetManager.SelectedFoodSamples.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
        }

 
    }
}
