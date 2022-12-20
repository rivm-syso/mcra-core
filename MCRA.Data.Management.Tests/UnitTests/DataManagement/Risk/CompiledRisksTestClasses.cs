using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled Risks when using CompiledDataManager.GetAllRisks
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerRisksTests : CompiledRisksTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getRisksDelegate = () => _compiledDataManager.GetAllRiskModels();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerRisksTests : CompiledRisksTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getRisksDelegate = () => _subsetManager.AllRiskModels;
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
        }
    }
}
