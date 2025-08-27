namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled HazardCharacterisations when using CompiledDataManager.GetAllHazardCharacterisations
    /// to retrieve the HazardCharacterisations
    /// </summary>
    [TestClass]
    public class CompiledHazardCharacterisationsTestCompiledDataManager : CompiledHazardCharacterisationsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getItemsDelegate = () => _compiledDataManager.GetAllHazardCharacterisations();
        }
    }

    /// <summary>
    /// Runs all tests for HazardCharacterisations when using SubsetManager.AllHazardCharacterisations
    /// to retrieve the HazardCharacterisations
    /// </summary>
    [TestClass]
    public class SubsetManagerAllHazardCharacterisationsTests : CompiledHazardCharacterisationsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getItemsDelegate = () => _subsetManager.AllHazardCharacterisations;
        }
    }
}
