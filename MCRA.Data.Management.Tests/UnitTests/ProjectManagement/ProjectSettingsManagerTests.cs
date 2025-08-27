using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Data.Management.Test.UnitTests.ProjectManagement {

    /// <summary>
    /// ProjectSettingsManagerTests
    /// </summary>
    [TestClass()]
    public class ProjectSettingsManagerTests {
        /// <summary>
        /// Test set is compute method of project settings manager.
        /// </summary>
        [TestMethod]
        public void ProjectSettingsManager_TestSetIsCompute() {
            var project = new ProjectDto();
            var manager = new ProjectSettingsManager();
            Assert.IsFalse(project.ConcentrationModelsSettings.IsCompute);
            manager.SetIsCompute(project, ActionType.ConcentrationModels, true);
            Assert.IsTrue(project.ConcentrationModelsSettings.IsCompute);
            manager.SetIsCompute(project, ActionType.ConcentrationModels, false);
            Assert.IsFalse(project.ConcentrationModelsSettings.IsCompute);
        }
    }
}
