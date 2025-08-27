using System.Xml;
using MCRA.General.Action.Serialization;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0001_02_Tests : ProjectSettingsSerializerTestsBase {

        //Test: KineticConversionFactors KineticModels and no TargetExposures
        // KineticConversionFactors module contains the following settings that are migrated from the KineticModels module:
        // - KCFSubgroupDependent
        // - ResampleKineticConversionFactors (from ResampleKineticModelParameters KineticModels module)
        [TestMethod]
        public void Patch_10_01_0001_02_KineticModelsModuleConfig_TestRemoveCodeKineticModel() {
            ModuleSettingsType moduleSettings = [
                ("KineticModels", [
                    ("CodeKineticModel", "EuroMix_Generic_PBTK_model_V6"),
                    ("KCFSubgroupDependent", "true"),
                    ("ResampleKineticModelParameters","true"),
                ])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            // Assert that setting does not exist anymore in transformed XML and that
            // other settings are not removed.
            var doc = new XmlDocument();
            doc.LoadXml(newXml);
            var xPathBase = "//Project/ModuleConfigurations/ModuleConfiguration[@module='PbkModels']/Settings/Setting";
            Assert.IsNull(doc.SelectSingleNode(xPathBase + "[@id='CodeKineticModel']"));
            Assert.AreEqual(
                "true",
                doc.SelectSingleNode(xPathBase + "[@id='ResamplePbkModelParameters']").InnerText
            );
        }

        [TestMethod]
        [DataRow("CombineInVivoPodInVitroDrms", "External", true)]
        [DataRow("CombineInVivoPodInVitroDrms", "Internal", true)]
        [DataRow("InVitroBmds", "External", true)]
        [DataRow("InVitroBmds", "Internal", true)]
        [DataRow("InVivoPods", "External", false)]
        [DataRow("InVivoPods", "Internal", true)]
        public void Patch_10_01_0001_02_HazardCharacterisationsModuleConfig_TestApplyKineticConversions(
            string calculationMethod,
            string targetLevel,
            bool expected
        ) {
            ModuleSettingsType moduleSettings = [
                ("HazardCharacterisations", [
                    ("TargetDosesCalculationMethod", calculationMethod),
                    ("TargetDoseLevelType", targetLevel),
                ])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.HazardCharacterisationsSettings;
            Assert.AreEqual(expected, modSettings.ApplyKineticConversions);
        }


        [TestMethod]
        public void Patch_10_01_0001_02_HazardCharacterisationsModuleConfig_TestApplyKineticConversions_NoModuleConfig() {
            var xmlOld = createMockSettingsXml(string.Empty, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.IsNotNull(settingsDto.HazardCharacterisationsSettings);
            Assert.IsFalse(settingsDto.HazardCharacterisationsSettings.ApplyKineticConversions);
        }

        [TestMethod]
        public void Patch_10_01_0001_02_HazardCharacterisationsModuleConfig_TestRemoveUseDoseResponseModels() {
            ModuleSettingsType moduleSettings = [
                ("HazardCharacterisations", [
                    ("TargetDoseLevelType", "External"),
                    ("UseDoseResponseModels", "true")
                ])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            // Assert that setting does not exist anymore in transformed XML and that
            // other settings are not removed.
            var doc = new XmlDocument();
            doc.LoadXml(newXml);
            var xPathBase = "//Project/ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting";
            Assert.IsNull(doc.SelectSingleNode(xPathBase + "[@id='UseDoseResponseModels']"));
            Assert.IsNotNull(doc.SelectSingleNode(xPathBase + "[@id='TargetDoseLevelType']"));
        }
    }
}
