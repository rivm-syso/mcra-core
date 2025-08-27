using MCRA.General.Action.Serialization;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_02_0004_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        [DataRow(ExposureCalculationMethod.ModelledConcentration)]
        [DataRow(ExposureCalculationMethod.MonitoringConcentration)]
        public void Patch_09_02_0004_MoveInternalConcentrationTypeToAssessmentSettings(ExposureCalculationMethod internalConcentrationType) {
            Func<ExposureCalculationMethod, string> createSettingsXml = (internalConcentrationType) =>
                "<AssessmentSettings>" +
                "</AssessmentSettings>" +
                "<MixtureSelectionSettings>" +
                $"<InternalConcentrationType>{internalConcentrationType}</InternalConcentrationType>" +
                "</MixtureSelectionSettings>";
            var xml = createMockSettingsXml(createSettingsXml(internalConcentrationType));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(internalConcentrationType, settingsDto.ExposureMixturesSettings.ExposureCalculationMethod);
        }
    }
}
