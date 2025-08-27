namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    /// <summary>
    /// Runs all tests for compiled focal food samples
    /// </summary>
    [TestClass]
    public class CompiledDataManagerFocalFoodConcentrationsTests : CompiledFocalFoodConcentrationsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getFocalFoodsAnalyticalMethodsDelegate = () => _compiledDataManager.GetAllFocalFoodAnalyticalMethods();
            _getAllFocalFoodSamplesDelegate = () => _compiledDataManager.GetAllFocalFoodSamples();
            _getAllFocalCommodityFoodsDelegate = () => _compiledDataManager.GetAllFocalCommodityFoods();
            _getFoodsDelegate = () => _compiledDataManager.GetAllFoods();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
        }
    }

    /// <summary>
    /// Runs all tests for focal food samples
    /// </summary>
    [TestClass]
    public class SubsetManagerFocalFoodConcentrationsTests : CompiledFocalFoodConcentrationsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _isSubSetManagerTest = true;
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getFoodsDelegate = () => _subsetManager.AllFoodsByCode;
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
            _getAllFocalFoodSamplesDelegate = () => _subsetManager.AllFocalCommoditySamples.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);
            _getAllFocalCommodityFoodsDelegate = () => _subsetManager.AllFocalCommodityFoods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);
            _getFocalFoodsAnalyticalMethodsDelegate = () => _subsetManager.AllFocalCommodityAnalyticalMethods;
        }
    }
}
