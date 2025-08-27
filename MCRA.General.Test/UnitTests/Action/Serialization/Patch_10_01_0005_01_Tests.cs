using MCRA.General.Action.Serialization;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class Patch_10_01_0005_01_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        public void Patch_10_01_0005_01_TestOldProject() {
            ModuleSettingsType moduleSettings = [
                ("PbkModels", [])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 4));

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(ExposureEventsGenerationMethod.RandomDailyEvents, settingsDto.PbkModelsSettings.ExposureEventsGenerationMethod);
        }

        [TestMethod]
        public void Patch_10_01_0005_01_TestDefault() {
            ModuleSettingsType moduleSettings = [
                ("PbkModels", [])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 5));

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(ExposureEventsGenerationMethod.DailyAverageEvents, settingsDto.PbkModelsSettings.ExposureEventsGenerationMethod);
        }
    }
}
