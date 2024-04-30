using MCRA.General.Action.Serialization;
using MCRA.General.ModuleDefinitions.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_01_0047_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        [DataRow("true", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("false", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("true", "AbsorptionFactorModel", InternalModelType.AbsorptionFactorModel)]
        [DataRow("false", "AbsorptionFactorModel", InternalModelType.AbsorptionFactorModel)]
        [DataRow("true", null, InternalModelType.PBKModel)]
        [DataRow("false", null, InternalModelType.AbsorptionFactorModel)]
        public void Patch_09_01_0047_TestRemoveUseKineticModel(
            string useKineticModel,
            string internalModelType,
            InternalModelType expectedModelType
        ) {
            Func<string, string, string> createSettingsXml = (use, model) =>
                "<KineticModelSettings>" +
                $"<UseKineticModel>{use}</UseKineticModel>" +
                (!string.IsNullOrEmpty(model) ? $"<InternalModelType>{model}</InternalModelType>" : string.Empty) +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(createSettingsXml(useKineticModel, internalModelType));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);

            Assert.AreEqual(expectedModelType, settingsDto.GetModuleConfiguration<KineticModelsModuleConfig>().InternalModelType);
        }
    }
}
