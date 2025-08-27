using MCRA.General.Action.Settings;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerConsumptionsTests : CompiledConsumptionsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getFoodsDelegate = () => _compiledDataManager.GetAllFoods();
            _getFoodSurveysDelegate = () => _compiledDataManager.GetAllFoodSurveys();
            _getIndividualsDelegate = () => _compiledDataManager.GetAllIndividuals();
            _getFoodConsumptionsDelegate = () => _compiledDataManager.GetAllFoodConsumptions();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerConsumptionsTests : CompiledConsumptionsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getFoodsDelegate = () => _subsetManager.AllFoodsByCode;
            _getFoodSurveysDelegate = () => _subsetManager.AllFoodSurveys;
            _getIndividualsDelegate = () => _subsetManager.AllIndividuals;
            _getFoodConsumptionsDelegate = () => _subsetManager.AllFoodConsumptions.ToList();
        }
    }
}
