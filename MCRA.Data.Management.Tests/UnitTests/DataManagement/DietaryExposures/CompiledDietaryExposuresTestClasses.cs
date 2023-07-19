using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled Dietary exposuress when using CompiledDataManager.GetAllDietaryExposures
    /// to retrieve the DietaryExposures
    /// </summary>
    [TestClass]
    public class CompiledDataManagerDietaryExposuresTests : CompiledDietaryExposuresTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getDietaryExposuresDelegate = () => _compiledDataManager.GetAllDietaryExposureModels();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerDietaryExposuresTests : CompiledDietaryExposuresTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getDietaryExposuresDelegate = () => _subsetManager.AllDietaryExposureModels;
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
        }
    }
}
