using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled DeterministicSubstanceConversionFactors when using compiledDataManager
    /// to retrieve the deterministic substance conversion factors.
    /// </summary>
    [TestClass]
    public class CompiledDataManagerDeterministicSubstanceConversionFactorsTests : CompiledDeterministicSubstanceConversionFactorsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getItemsDelegate = () => _compiledDataManager.GetAllDeterministicSubstanceConversionFactors();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerDeterministicSubstanceConversionFactorsTests : CompiledDeterministicSubstanceConversionFactorsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getItemsDelegate = () => _subsetManager.AllDeterministicSubstanceConversionFactors;
        }
    }
}
