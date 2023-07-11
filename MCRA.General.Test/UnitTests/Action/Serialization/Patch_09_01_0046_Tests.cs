using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_01_0046_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        [DataRow("CosmosV4", "EuroMix_Generic_PBTK_model_V5")]
        [DataRow("CosmosV6", "EuroMix_Generic_PBTK_model_V6")]
        [DataRow("PBPKModel_BPA", "EuroMix_Bisphenols_PBPK_model_V1")]
        [DataRow("PBPKModel_BPA_Reimplementation", "EuroMix_Bisphenols_PBPK_model_V2")]
        [DataRow("XXX", "XXX")]
        public void Patch_09_01_0046_TestRecodeKineticModels(string oldCode, string newCode) {
            Func<string, string> createSettingsXml = (code) =>
                "<KineticModelSettings>" +
                $"<CodeModel>{code}</CodeModel>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(createSettingsXml(oldCode));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(newCode, settingsDto.KineticModelSettings.CodeModel);
        }
    }
}
