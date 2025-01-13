using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class Patch_10_01_0005_00_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        public void Patch_10_01_0004_00_Test() {
            ModuleSettingsType moduleSettings = [
                ("Populations", []),
                ("ActiveSubstances", []),
                ("DietaryExposures", []),
                ("HazardCharacterisations", []),
                ("FoodConversions", []),
                ("DoseResponseModels", []),
                ("Consumptions", []),
                ("ExposureMixtures", [])
            ];

            var calculationTypes = new HashSet<string> {
                "ActiveSubstances",
                "Populations",
                "DoseResponseModels",
                "HazardCharacterisations"
            };

            var xmlOld = createMockSettingsXml(
                moduleSettings,
                new(10, 1, 1),
                ActionType.DietaryExposures,
                calculationTypes
            );
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            //loop through all action types
            foreach (var t in Enum.GetValues<ActionType>().Where(t => (int)t >= 0)) {
                var conf = settingsDto.GetModuleConfiguration(t);
                if (conf != null) {
                    Assert.AreEqual(calculationTypes.Contains(t.ToString()), conf.IsCompute);
                }
            }
        }
    }
}
