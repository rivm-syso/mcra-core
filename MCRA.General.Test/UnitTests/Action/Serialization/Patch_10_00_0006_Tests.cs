using MCRA.General.Action.Serialization;
using MCRA.General.ModuleDefinitions.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0006_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        [DataRow("ModelledConcentration", ExposureCalculationMethod.ModelledConcentration)]
        [DataRow("MonitoringConcentration", ExposureCalculationMethod.MonitoringConcentration)]
        public void Patch_10_00_0006_InternalConcentrationType_Rename(
            string oldValue,
            ExposureCalculationMethod newValue
        ) {
            var settingsXml =
                "<AssessmentSettings>" +
                  $"<InternalConcentrationType>{oldValue}</InternalConcentrationType>" +
                "</AssessmentSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 5));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                newValue,
                settingsDto.GetModuleConfiguration<ExposureMixturesModuleConfig>().ExposureCalculationMethod
            );
        }
    }
}
