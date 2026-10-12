using MCRA.General.Action.Serialization;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class Patch_10_03_0003_00_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        public void Patch_10_03_0003_00_TestOldProject() {
            ModuleSettingsType moduleSettings = [
                ("EnvironmentalBurdenOfDisease", [])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 3, 2));

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(PAFCalculationMethod.PersonWeightedAF, settingsDto.EnvironmentalBurdenOfDiseaseSettings.PAFCalculationMethod);
        }

        [TestMethod]
        public void Patch_10_03_0003_00_TestDefault() {
            ModuleSettingsType moduleSettings = [
                ("EnvironmentalBurdenOfDisease", [])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 3, 3));

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(PAFCalculationMethod.Levin, settingsDto.EnvironmentalBurdenOfDiseaseSettings.PAFCalculationMethod);
        }
    }
}
