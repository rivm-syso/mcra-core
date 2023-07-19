using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled substances when using CompiledDataManager.GetAllCompounds
    /// to retrieve the substances
    /// </summary>
    [TestClass]
    public class CompiledEffectsTestCompiledDataManager : CompiledEffectsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getEffectsDelegate = () => _compiledDataManager.GetAllEffects();
        }
    }

    /// <summary>
    /// Runs all tests for compiled substances when using SubsetManager.AllCompoundsByCode
    /// to retrieve the substances
    /// </summary>
    [TestClass]
    public class SubsetManagerAllEffectsTests : CompiledEffectsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getEffectsDelegate = () => _subsetManager.AllEffects;
        }
    }

}
