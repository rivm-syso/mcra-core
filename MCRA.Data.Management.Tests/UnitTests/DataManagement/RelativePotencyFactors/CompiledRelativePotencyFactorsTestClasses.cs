using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerRelativePotencyFactorsTests: CompiledRelativePotencyFactorsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getItemsDelegate = () => _compiledDataManager.GetAllRelativePotencyFactors();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerRelativePotencyFactorsTests : CompiledRelativePotencyFactorsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getItemsDelegate = () => _subsetManager.AllRelativePotencyFactors;
        }
    }
}
