using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_02_0007_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        public void Patch_09_02_0007_TestExisting() {
            var settingsXml =
                "<MixtureSelectionSettings></MixtureSelectionSettings>"
               + "<HumanMonitoringSettings></HumanMonitoringSettings>";

            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            var config = settingsDto.HumanMonitoringAnalysisSettings;
            Assert.IsTrue(config.McrAnalysis);
            Assert.AreEqual(ExposureApproachType.RiskBased, config.McrExposureApproachType);
            Assert.IsTrue(config.StandardiseUrine);
            Assert.AreEqual(StandardiseUrineMethod.SpecificGravity, config.StandardiseUrineMethod);
        }

        [TestMethod]
        public void Patch_09_02_0007_TestEmpty() {
            var xml = createMockSettingsXml();
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            var config = settingsDto.HumanMonitoringAnalysisSettings;
            Assert.IsTrue(config.McrAnalysis);
            Assert.AreEqual(ExposureApproachType.RiskBased, config.McrExposureApproachType);
            Assert.IsTrue(config.StandardiseUrine);
            Assert.AreEqual(StandardiseUrineMethod.SpecificGravity, config.StandardiseUrineMethod);
        }
    }
}
