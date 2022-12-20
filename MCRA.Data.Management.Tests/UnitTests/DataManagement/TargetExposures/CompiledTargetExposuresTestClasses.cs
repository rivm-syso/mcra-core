using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerTargetExposuresTests : CompiledTargetExposuresTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getTargetExposuresDelegate = () => _compiledDataManager.GetAllTargetExposureModels();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerTargetExposuresTests : CompiledTargetExposuresTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getTargetExposuresDelegate = () => _subsetManager.AllTargetExposureModels;
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
        }
    }
}
