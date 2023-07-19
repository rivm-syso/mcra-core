using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerDoseResponseDataTests : CompiledDoseResponseDataTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getExperimentsDelegate = () => _compiledDataManager.GetAllDoseResponseExperiments();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
            _getResponsesDelegate = () => _compiledDataManager.GetAllResponses();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerDoseResponseDataTests : CompiledDoseResponseDataTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getExperimentsDelegate = () => _subsetManager.AllDoseResponseExperiments.ToDictionary(e => e.Code, StringComparer.OrdinalIgnoreCase);
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
            _getResponsesDelegate = () => _subsetManager.AllResponses;
        }
    }
}
