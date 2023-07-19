using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class SubsetManagerTestsBase : CompiledTestsBase {

        protected SubsetManager _subsetManager;
        protected ProjectDto _project;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _project = new ProjectDto();
            _subsetManager = new SubsetManager(_compiledDataManager, _project);
        }
    }
}
