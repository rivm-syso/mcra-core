using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_02_0004_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        [DataRow(InternalConcentrationType.ModelledConcentration)]
        [DataRow(InternalConcentrationType.MonitoringConcentration)]
        public void Patch_09_02_0004_MoveInternalConcentrationTypeToAssessmentSettings(InternalConcentrationType internalConcentrationType) {
            Func<InternalConcentrationType, string> createSettingsXml = (internalConcentrationType) =>
                "<AssessmentSettings>" +
                "</AssessmentSettings>" +
                "<MixtureSelectionSettings>" +
                $"<InternalConcentrationType>{internalConcentrationType}</InternalConcentrationType>" +
                "</MixtureSelectionSettings>";
            var xml = createMockSettingsXml(createSettingsXml(internalConcentrationType));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(internalConcentrationType, settingsDto.AssessmentSettings.InternalConcentrationType);
        }
    }
}
