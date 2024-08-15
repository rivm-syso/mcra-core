using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0001_Tests : ProjectSettingsSerializerTestsBase {

        private string generateModuleSettings(string name, List<(string key, string value)> settings) {
            var result = $"<ModuleConfiguration module=\"{name}\">";
            result += "<Settings>";
            foreach (var setting in settings) {
                result += $"<Setting id=\"{setting.key}\">{setting.value}</Setting>";
            }
            result += "</Settings>";
            result += "</ModuleConfiguration>";
            return result;
        }

        //Test: PbkModels KineticModels and no TargetExposures
        // PbkModels module contains the following settings that are migrated from the KineticModels module:
        // - CodeKineticModel
        // - NumberOfDays
        // - NonStationaryPeriod
        // - UseParameterVariability
        // - NumberOfDosesPerDayNonDietaryDermal
        // - NumberOfDosesPerDayNonDietaryInhalation
        // - NumberOfDosesPerDayNonDietaryOral
        // - SelectedEvents
        // - SpecifyEvents
        // - ResamplePbkModelParameters (from ResampleKineticModelParameters KineticModels module
        [TestMethod]
        public void Patch_10_01_0001_PbkModelsModuleConfigTest() {
            var settings = new List<(string key, string value)>() {
                ("CodeKineticModel", "EuroMix_Generic_PBTK_model_V6"),
                ("NumberOfDays", "129"),
                ("NonStationaryPeriod", "13"),
                ("NumberOfDosesPerDay","2"),
                ("NumberOfDosesPerDayNonDietaryOral","3"),
                ("NumberOfDosesPerDayNonDietaryDermal","4"),
                ("NumberOfDosesPerDayNonDietaryInhalation","5"),
                ("UseParameterVariability","true"),
                ("ResampleKineticModelParameters","true"),
                ("SpecifyEvents","true"),
                ("SelectedEvents","<Value>1</Value><Value>22</Value><Value>333</Value><Value>4444</Value>"),
                ("InternalModelType","PBKModel"),
                ("DermalAbsorptionFactor","7387383")
            };

            var oldModuleSettingsXml = generateModuleSettings("KineticModels", settings);
            var xmlOld = createMockSettingsXml(oldModuleSettingsXml, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.PbkModelsSettings;

            Assert.IsNotNull(settings);
            Assert.AreEqual(129, modSettings.NumberOfDays);
            Assert.AreEqual(2, modSettings.NumberOfDosesPerDay);
            Assert.AreEqual(13, modSettings.NonStationaryPeriod);
            Assert.AreEqual("EuroMix_Generic_PBTK_model_V6", modSettings.CodeKineticModel);
            Assert.IsTrue(modSettings.UseParameterVariability);
            Assert.AreEqual(3, modSettings.NumberOfDosesPerDayNonDietaryOral);
            Assert.AreEqual(4, modSettings.NumberOfDosesPerDayNonDietaryDermal);
            Assert.AreEqual(5, modSettings.NumberOfDosesPerDayNonDietaryInhalation);
            Assert.AreEqual("1 22 333 4444", string.Join(' ', modSettings.SelectedEvents));
            Assert.IsTrue(modSettings.SpecifyEvents);
            Assert.IsTrue(modSettings.ResamplePbkModelParameters);
            Assert.AreEqual("PBKModel", settingsDto.TargetExposuresSettings.InternalModelType.ToString());
        }

        //Test: PbkModels KineticModels and TargetExposures
        [TestMethod]
        public void Patch_10_01_0001_PbkModelsModuleTargetExposureConfigTest() {
            var settings = new List<(string key, string value)>() {
                ("CodeKineticModel", "EuroMix_Generic_PBTK_model_V6"),
                ("NumberOfDays", "129"),
                ("NonStationaryPeriod", "13"),
                ("NumberOfDosesPerDay","2"),
                ("NumberOfDosesPerDayNonDietaryOral","3"),
                ("NumberOfDosesPerDayNonDietaryDermal","4"),
                ("NumberOfDosesPerDayNonDietaryInhalation","5"),
                ("UseParameterVariability","true"),
                ("ResampleKineticModelParameters","true"),
                ("SpecifyEvents","true"),
                ("SelectedEvents","<Value>1</Value><Value>22</Value><Value>333</Value><Value>4444</Value>"),
                ("InternalModelType","PBKModel"),
                ("DermalAbsorptionFactor","7387383")
            };
            var settingsTargetExposures = new List<(string key, string value)>() {
                ("CodeCompartment", "Liver"),
            };

            var oldModuleSettingsXml = generateModuleSettings("KineticModels", settings);
            var oldModuleSettingsXmTargetExposures = generateModuleSettings("TargetExposures", settingsTargetExposures);
            oldModuleSettingsXml = oldModuleSettingsXml + oldModuleSettingsXmTargetExposures;

            var xmlOld = createMockSettingsXml(oldModuleSettingsXml, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.PbkModelsSettings;
            Assert.IsNotNull(settings);
            Assert.AreEqual(129, modSettings.NumberOfDays);
            Assert.AreEqual(2, modSettings.NumberOfDosesPerDay);
            Assert.AreEqual(13, modSettings.NonStationaryPeriod);
            Assert.AreEqual("EuroMix_Generic_PBTK_model_V6", modSettings.CodeKineticModel);
            Assert.IsTrue(modSettings.UseParameterVariability);
            Assert.AreEqual(3, modSettings.NumberOfDosesPerDayNonDietaryOral);
            Assert.AreEqual(4, modSettings.NumberOfDosesPerDayNonDietaryDermal);
            Assert.AreEqual(5, modSettings.NumberOfDosesPerDayNonDietaryInhalation);
            Assert.AreEqual("1 22 333 4444", string.Join(' ', modSettings.SelectedEvents));
            Assert.IsTrue(modSettings.SpecifyEvents);
            Assert.IsTrue(modSettings.ResamplePbkModelParameters);
            Assert.AreEqual("PBKModel", settingsDto.TargetExposuresSettings.InternalModelType.ToString());
        }
    }
}
