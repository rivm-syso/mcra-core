using MCRA.General.Action.Settings.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerActiveSubstancesTests : CompiledActiveSubstancesTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getItemsDelegate = () => _compiledDataManager.GetAllActiveSubstanceModels();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerActiveSubstancesTests : CompiledActiveSubstancesTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getItemsDelegate = () => _subsetManager.AllActiveSubstances.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase);
        }
    }
}
