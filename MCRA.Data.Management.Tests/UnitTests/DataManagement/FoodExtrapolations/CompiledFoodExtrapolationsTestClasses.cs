using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerFoodExtrapolationsTests : CompiledFoodExtrapolationsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getFoodsDelegate = () => _compiledDataManager.GetAllFoods();
            _getFoodExtrapolationsDelegate = () => _compiledDataManager.GetAllFoodExtrapolations();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerFoodExtrapolationsTests : CompiledFoodExtrapolationsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getFoodsDelegate = () => _subsetManager.AllFoodsByCode;
            _getFoodExtrapolationsDelegate = () => _subsetManager.AllFoodExtrapolations;
        }
    }
}
