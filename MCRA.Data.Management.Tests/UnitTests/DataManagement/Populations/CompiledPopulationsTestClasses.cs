using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled Populations when using CompiledDataManager.GetAllPopulations
    /// to retrieve the Populations
    /// </summary>
    [TestClass]
    public class CompiledDataManagerPopulationsTests : CompiledPopulationsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getPopulationsDelegate = () => _compiledDataManager.GetAllPopulations();
        }
    }

    /// <summary>
    /// Runs all tests for Populations when using SubsetManager.Populations
    /// to retrieve the Populations
    /// </summary>
    [TestClass]
    public class SubsetManagerPopulationsTests : CompiledPopulationsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getPopulationsDelegate = () => _subsetManager.AllPopulations.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);
        }
    }
}
