using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Test.Mock.MockProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic.SettingsSummary {
    [TestClass]
    public class SettingsCompareTests {
        [TestMethod]
        public void CreateMoqProjectTest() {
            var project = new MockProject();

            Assert.IsNotNull(project);

            var mockedSettings = project.GetMockedObject(typeof(EffectsModuleConfig));
            Assert.IsNotNull(mockedSettings);
            Assert.IsEmpty(mockedSettings.Invocations);

        }
    }
}
