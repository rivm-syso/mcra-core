using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Assert.IsTrue(!project.CalculationActionTypes.Contains(ActionType.ConcentrationModels));
            manager.SetIsCompute(project, ActionType.ConcentrationModels, true);
            Assert.IsTrue(project.CalculationActionTypes.Contains(ActionType.ConcentrationModels));
            manager.SetIsCompute(project, ActionType.ConcentrationModels, false);
            Assert.IsTrue(!project.CalculationActionTypes.Contains(ActionType.ConcentrationModels));
        }
    }
}
