using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerDoseResponseModelsTests : CompiledDoseResponseModelsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getResponseModelsDelegate = () => _compiledDataManager.GetAllDoseResponseModels();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
            _getResponsesDelegate = () => _compiledDataManager.GetAllResponses();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerDoseResponseModelsTests : CompiledDoseResponseModelsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getResponseModelsDelegate = () => _subsetManager.AllDoseResponseModels;
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
            _getResponsesDelegate = () => _subsetManager.AllResponses;
        }
    }
}
