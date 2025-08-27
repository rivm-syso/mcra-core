namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled substances when using CompiledDataManager.GetAllCompounds
    /// to retrieve the substances
    /// </summary>
    [TestClass]
    public class CompiledSubstancesTestCompiledDataManager : CompiledSubstancesTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getSubstancesDelegate = () => _compiledDataManager.GetAllCompounds();
        }
    }

    /// <summary>
    /// Runs all tests for compiled substances when using SubsetManager.AllCompoundsByCode
    /// to retrieve the substances
    /// </summary>
    [TestClass]
    public class SubsetManagerAllSubstancesByCodeTests : CompiledSubstancesTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getSubstancesDelegate = () => _subsetManager.AllCompoundsByCode;
        }
    }

    /// <summary>
    /// Runs all tests for compiled substances when using SubsetManager.Allsubstances
    /// (cast to a dictionary by substance code) to retrieve the substances
    /// </summary>
    [TestClass]
    public class SubsetManagerAllSubstancesTests : CompiledSubstancesTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getSubstancesDelegate = () => _subsetManager.AllCompounds.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Runs all tests for compiled substances when using SubsetManager.AvailableCompounds
    /// (cast to a dictionary by substance code) to retrieve the substances
    /// </summary>
    [TestClass]
    public class SubsetManagerAvailableSubstancesTests : CompiledSubstancesTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getSubstancesDelegate = () => _subsetManager.AllCompounds.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);
        }
    }
}
