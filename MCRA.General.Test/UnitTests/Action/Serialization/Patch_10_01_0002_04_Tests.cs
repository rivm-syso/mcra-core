using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0002_04_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        [DataRow("UseConsumptions", DustExposuresIndividualGenerationMethod.UseDietaryExposures)]
        [DataRow("Simulate", DustExposuresIndividualGenerationMethod.Simulate)]
        public void Patch_10_01_0002_04_Test(
            string oldGenerationMethod,
            DustExposuresIndividualGenerationMethod expectedGenerationMethod
        ) {
            ModuleSettingsType moduleSettings = [
                ("DustExposures",
                    [
                        ("DustExposuresIndividualGenerationMethod", oldGenerationMethod.ToString())
                    ]
                ),
                ("Individuals", [
                    ("NumberOfSimulatedIndividuals", "111"),
                    ])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(expectedGenerationMethod, settingsDto.DustExposuresSettings.DustExposuresIndividualGenerationMethod);
            Assert.AreEqual(111, settingsDto.IndividualsSettings.NumberOfSimulatedIndividuals);
        }

        [TestMethod]
        public void Patch_10_01_0002_04_Test_NoSettings() {
            ModuleSettingsType moduleSettings = [
                ("TargetExposures", [])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(DustExposuresIndividualGenerationMethod.Simulate, settingsDto.DustExposuresSettings.DustExposuresIndividualGenerationMethod);
        }
    }
}
