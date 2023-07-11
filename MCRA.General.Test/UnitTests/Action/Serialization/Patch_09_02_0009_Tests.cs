using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_02_0009_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        public void Patch_09_02_0009_TestHumanMonitoringSurveysSettings() {
            var hbmSurveyCodes = new[] { "FAasdgoa040329", "AGDS332", "43290032w" };
            var settingsXml =
                "<HumanMonitoringSettings>" +
                "  <SurveyCodes>" +
                $"   <string>{string.Join("</string><string>", hbmSurveyCodes)}</string>" +
                "  </SurveyCodes>" +
                "</HumanMonitoringSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(9, 2, 8));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            CollectionAssert.AreEqual(hbmSurveyCodes, settingsDto.ScopeKeysFilters[0].SelectedCodes.ToList());
        }
    }
}
