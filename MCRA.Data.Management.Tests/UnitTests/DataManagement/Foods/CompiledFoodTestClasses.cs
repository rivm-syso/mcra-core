using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled foods when using CompiledDataManager.GetAllFoods
    /// to retrieve the foods
    /// </summary>
    [TestClass]
    public class CompiledFoodsTests : CompiledFoodTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getFoodsDelegate = () => _compiledDataManager.GetAllFoods();
        }
    }

    /// <summary>
    /// Runs all tests for compiled foods when using SubsetManager.AllFoodsByCode
    /// to retrieve the foods
    /// </summary>
    [TestClass]
    public class SubsetManagerAllFoodsByCodeTests : CompiledFoodTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getFoodsDelegate = () => _subsetManager.AllFoodsByCode;
        }
    }

    /// <summary>
    /// Runs all tests for compiled foods when using SubsetManager.AllFoods
    /// (cast to a dictionary by food code) to retrieve the foods
    /// </summary>
    [TestClass]
    public class SubsetManagerAllFoodsTests : CompiledFoodTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getFoodsDelegate = () => _subsetManager.AllFoods.ToDictionary(f => f.Code, StringComparer.OrdinalIgnoreCase);
        }
    }
}
