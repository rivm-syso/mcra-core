using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0001_01_Tests : ProjectSettingsSerializerTestsBase {

        //Test: KineticConversionFactors KineticModels and no TargetExposures
        // KineticConversionFactors module contains the following settings that are migrated from the KineticModels module:
        // - KCFSubgroupDependent
        // - ResampleKineticConversionFactors (from ResampleKineticModelParameters KineticModels module)
        [TestMethod]
        public void Patch_10_01_0001_01_KineticConversionFactorsModuleConfigTest() {

            ModuleSettingsType moduleSettings = [
                ("KineticModels", [
                    ("KCFSubgroupDependent", "true"),
                    ("ResampleKineticModelParameters","true"),
                ])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.KineticConversionFactorsSettings;

            Assert.IsNotNull(modSettings);
            Assert.IsTrue(modSettings.KCFSubgroupDependent);
            Assert.IsTrue(modSettings.ResampleKineticConversionFactors);
        }

        //Test: KineticConversionFactors KineticModels and TargetExposures
        [TestMethod]
        public void Patch_10_01_0001_01_KineticConversionFactorssModuleTargetExposureConfigTest() {

            ModuleSettingsType moduleSettings =
                [
                    ("KineticModels", [
                        ("KCFSubgroupDependent", "true"),
                        ("ResampleKineticModelParameters","true"),
                    ]),
                    ("TargetExposures", [
                        ("CodeCompartment", "Liver")
                    ])
                ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.KineticConversionFactorsSettings;

            Assert.IsNotNull(modSettings);
            Assert.IsTrue(modSettings.KCFSubgroupDependent);
            Assert.IsTrue(modSettings.ResampleKineticConversionFactors);
        }



        //Test: KineticConversionFactors KineticModels and HazardCharacterisations
        [TestMethod]
        public void Patch_10_01_0001_01_PbKineticConversionFactorsModuleHazardCharacterisationConfigTest() {
            ModuleSettingsType moduleSettings = [
                ("KineticModels", [
                    ("KCFSubgroupDependent", "true"),
                    ("ResampleKineticModelParameters","true"),
                ]),
                ("HazardCharacterisations", [
                    ("CodeCompartment", "Liver")
                ])
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.KineticConversionFactorsSettings;

            Assert.IsNotNull(modSettings);
            Assert.IsTrue(modSettings.KCFSubgroupDependent);
            Assert.IsTrue(modSettings.ResampleKineticConversionFactors);
        }
    }
}
