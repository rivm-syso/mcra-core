using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_02_0008_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        public void Patch_09_02_0008_TestExistingFalse() {
            var settingsXml =
                "<EffectModelSettings>" +
                "  <CumulativeRisk>false</CumulativeRisk>" +
                "</EffectModelSettings>";
            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsFalse(settingsDto.RisksSettings.CumulativeRisk);
        }

        [TestMethod]
        public void Patch_09_02_0008_TestNonExisting() {
            var settingsXml =
                "<EffectModelSettings></EffectModelSettings>";
            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsTrue(settingsDto.RisksSettings.CumulativeRisk);
        }

        [TestMethod]
        public void Patch_09_02_0008_TestEmptySettingsFile() {
            var xml = createMockSettingsXml();
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsFalse(settingsDto.RisksSettings.CumulativeRisk);
        }
    }
}
