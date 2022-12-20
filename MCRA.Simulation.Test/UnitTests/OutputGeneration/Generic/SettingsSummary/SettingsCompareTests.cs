using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Test.Mock.MockProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic.SettingsSummary {
    [TestClass]
    public class SettingsCompareTests {
        [TestMethod]
        public void CreateMoqProjectTest() {
            var project = new MockProject();

            Assert.IsNotNull(project);

            var mockedSettings = project.GetMockedObject(typeof(ConcentrationModelSettingsDto));
            Assert.IsNotNull(mockedSettings);
            Assert.AreEqual(0, mockedSettings.Invocations.Count);

        }
    }
}
