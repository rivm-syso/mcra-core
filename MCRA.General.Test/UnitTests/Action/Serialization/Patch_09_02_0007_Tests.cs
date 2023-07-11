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
            Assert.IsTrue(settingsDto.MixtureSelectionSettings.IsMcrAnalysis);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.MixtureSelectionSettings.McrExposureApproachType);
            Assert.IsTrue(settingsDto.HumanMonitoringSettings.StandardiseUrine);
            Assert.AreEqual(StandardiseUrineMethod.SpecificGravity, settingsDto.HumanMonitoringSettings.StandardiseUrineMethod);
        }

        [TestMethod]
        public void Patch_09_02_0007_TestEmpty() {
            var xml = createMockSettingsXml();
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsTrue(settingsDto.MixtureSelectionSettings.IsMcrAnalysis);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.MixtureSelectionSettings.McrExposureApproachType);
            Assert.IsTrue(settingsDto.HumanMonitoringSettings.StandardiseUrine);
            Assert.AreEqual(StandardiseUrineMethod.SpecificGravity, settingsDto.HumanMonitoringSettings.StandardiseUrineMethod);
        }
    }
}
