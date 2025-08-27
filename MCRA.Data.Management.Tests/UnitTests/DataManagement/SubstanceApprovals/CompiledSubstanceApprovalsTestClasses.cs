namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled substance approvals.
    /// </summary>
    [TestClass]
    public class CompiledDataManagerSubstanceApprovalsTests : CompiledSubstanceApprovalsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getItemsDelegate = () => _compiledDataManager.GetAllSubstanceApprovals();
        }
    }
}
