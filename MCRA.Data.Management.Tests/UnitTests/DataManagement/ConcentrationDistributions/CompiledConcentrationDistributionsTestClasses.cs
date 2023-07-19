using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerConcentrationDistributionsTests : CompiledConcentrationDistributionsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getConcentrationDistributionsDelegate = () => _compiledDataManager.GetAllConcentrationDistributions();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerConcentrationDistributionsTests : CompiledConcentrationDistributionsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _isSubSetManagerTest = true;
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getConcentrationDistributionsDelegate = () => _subsetManager.AllConcentrationDistributions;
        }
    }
}
