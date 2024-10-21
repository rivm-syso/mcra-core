using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0001_00_Tests : ProjectSettingsSerializerTestsBase {

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

            ModuleSettingsType moduleSettings = [
                ("KineticModels", [
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
                ])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.PbkModelsSettings;

            Assert.IsNotNull(modSettings);
            Assert.AreEqual(129, modSettings.NumberOfDays);
            Assert.AreEqual(13, modSettings.NonStationaryPeriod);
            // CodeKineticModel removed in version 10.0.2
            //Assert.AreEqual("EuroMix_Generic_PBTK_model_V6", modSettings.CodeKineticModel);
            Assert.IsTrue(modSettings.UseParameterVariability);
            Assert.AreEqual(3, modSettings.NumberOfDosesPerDayNonDietaryOral);
            Assert.AreEqual(4, modSettings.NumberOfDosesPerDayNonDietaryDermal);
            Assert.AreEqual(5, modSettings.NumberOfDosesPerDayNonDietaryInhalation);
            Assert.AreEqual("1 22 333 4444", string.Join(' ', modSettings.SelectedEvents));
            Assert.IsTrue(modSettings.SpecifyEvents);
            Assert.IsTrue(modSettings.ResamplePbkModelParameters);
        }

        //Test: PbkModels KineticModels and TargetExposures
        [TestMethod]
        public void Patch_10_01_0001_PbkModelsModuleTargetExposureConfigTest() {

            ModuleSettingsType moduleSettings =
                [
                    ("KineticModels", [
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
                    ]),
                    ("TargetExposures", [
                        ("CodeCompartment", "Liver")
                    ])
                ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.PbkModelsSettings;

            Assert.IsNotNull(modSettings);
            Assert.AreEqual(129, modSettings.NumberOfDays);
            Assert.AreEqual(13, modSettings.NonStationaryPeriod);
            // CodeKineticModel removed in version 10.0.2
            //Assert.AreEqual("EuroMix_Generic_PBTK_model_V6", modSettings.CodeKineticModel);
            Assert.IsTrue(modSettings.UseParameterVariability);
            Assert.AreEqual(3, modSettings.NumberOfDosesPerDayNonDietaryOral);
            Assert.AreEqual(4, modSettings.NumberOfDosesPerDayNonDietaryDermal);
            Assert.AreEqual(5, modSettings.NumberOfDosesPerDayNonDietaryInhalation);
            Assert.AreEqual("1 22 333 4444", string.Join(' ', modSettings.SelectedEvents));
            Assert.IsTrue(modSettings.SpecifyEvents);
            Assert.IsTrue(modSettings.ResamplePbkModelParameters);
        }



        //Test: PbkModels KineticModels and HazardCharacterisations
        [TestMethod]
        public void Patch_10_01_0001_PbkModelsModuleHazardCharacterisationConfigTest() {
            ModuleSettingsType moduleSettings = [
                ("KineticModels", [
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
                    ]),
                ("HazardCharacterisations", [
                    ("CodeCompartment", "Liver")
                ])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.PbkModelsSettings;

            Assert.IsNotNull(modSettings);
            Assert.AreEqual(129, modSettings.NumberOfDays);
            Assert.AreEqual(13, modSettings.NonStationaryPeriod);
            // CodeKineticModel removed in version 10.0.2
            //Assert.AreEqual("EuroMix_Generic_PBTK_model_V6", modSettings.CodeKineticModel);
            Assert.IsTrue(modSettings.UseParameterVariability);
            Assert.AreEqual(3, modSettings.NumberOfDosesPerDayNonDietaryOral);
            Assert.AreEqual(4, modSettings.NumberOfDosesPerDayNonDietaryDermal);
            Assert.AreEqual(5, modSettings.NumberOfDosesPerDayNonDietaryInhalation);
            Assert.AreEqual("1 22 333 4444", string.Join(' ', modSettings.SelectedEvents));
            Assert.IsTrue(modSettings.SpecifyEvents);
            Assert.IsTrue(modSettings.ResamplePbkModelParameters);
        }
    }
}
