using MCRA.General.Action.Serialization;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0004_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        [DataRow("MarginOfExposure", RiskMetricType.HazardExposureRatio)]
        [DataRow("HazardIndex", RiskMetricType.ExposureHazardRatio)]
        public void Patch_10_00_0004_RiskMetricType_RenameMarginOfExposure(string oldValue, RiskMetricType newValue) {
            var settingsXml =
                "<RisksSettings>" +
                  $"<RiskMetricType>{oldValue}</RiskMetricType>" +
                "</RisksSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 3));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                newValue,
                settingsDto.RisksSettings.RiskMetricType
            );
        }
    }
}
