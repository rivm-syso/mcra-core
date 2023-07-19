using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerFoodTranslationsTests : CompiledFoodTranslationsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getFoodsDelegate = () => _compiledDataManager.GetAllFoods();
            _getFoodTranslationsDelegate = () => _compiledDataManager.GetAllFoodTranslations();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerFoodTranslationsTests : CompiledFoodTranslationsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getFoodsDelegate = () => _subsetManager.AllFoodsByCode;
            _getFoodTranslationsDelegate = () => _subsetManager.AllFoodTranslations;
        }
    }
}
