using System.Xml;
using MCRA.General.Action.Serialization;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0002_02_Tests : ProjectSettingsSerializerTestsBase {

        /// <summary>
        /// Test: KineticModels: remove InternalModelType
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="hasSetting"></param>
        [TestMethod]
        [DataRow("KineticModels", true)]
        [DataRow("KineticModels", false)]
        [DataRow("PbkModels", true)]
        [DataRow("PbkModels", false)]
        public void Patch_10_01_0002_02_TestKineticModels_RemoveInternalModelType(
            string actionType,
            bool hasSetting
        ) {
            ModuleSettingsType moduleSettings = [
                (actionType,
                    hasSetting ? [("InternalModelType", "AbsorptionFactorModel")] : []
                ),
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            // Assert that setting does not exist anymore in transformed XML
            var doc = new XmlDocument();
            doc.LoadXml(newXml);
            var xPathBase = $"//Project/ModuleConfigurations/ModuleConfiguration[@module='{actionType}']/Settings/Setting";
            Assert.IsNull(doc.SelectSingleNode(xPathBase + "[@id='InternalModelType']"));
        }


        /// <summary>
        /// Test
        /// </summary>
        [TestMethod]
        [DataRow("AbsorptionFactorModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("ConversionFactorModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("PBKModel", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("ConversionFactorModel", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("PBKModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        public void Patch_10_01_0002_02_Test_TargetLevelType_Risks_Hbm(
            string oldTargetExposureInternalModelType,
            string oldHazardCharacterisationInternalModelType,
            InternalModelType expectedHazardCharacterisationInternalModelType
        ) {
            var xmlOld = createActionXml(
                ActionType.Risks,
                "MonitoringConcentration",
                "Internal",
                oldTargetExposureInternalModelType,
                oldHazardCharacterisationInternalModelType
            );
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            Assert.AreEqual(TargetLevelType.Internal, settingsDto.HazardCharacterisationsSettings.TargetDoseLevelType);
            Assert.AreEqual(expectedHazardCharacterisationInternalModelType, settingsDto.HazardCharacterisationsSettings.InternalModelType);
        }

        /// <summary>
        /// Test
        /// </summary>
        [TestMethod]
        [DataRow("AbsorptionFactorModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("ConversionFactorModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("PBKModel", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("ConversionFactorModel", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("PBKModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        public void Patch_10_01_0002_02_Test_TargetLevelType_Hbm(
            string oldTargetExposureInternalModelType,
            string oldHazardCharacterisationInternalModelType,
            InternalModelType expectedHazardCharacterisationInternalModelType
        ) {
            var xmlOld = createActionXml(
                ActionType.HumanMonitoringAnalysis,
                "ModelledConcentration",
                //This setting is not tested
                //"External",
                "Internal",
                oldTargetExposureInternalModelType,
                oldHazardCharacterisationInternalModelType
            );
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            Assert.AreEqual(TargetLevelType.Internal, settingsDto.HazardCharacterisationsSettings.TargetDoseLevelType);
            Assert.AreEqual(expectedHazardCharacterisationInternalModelType, settingsDto.HazardCharacterisationsSettings.InternalModelType);
        }

        /// <summary>
        /// Test
        /// </summary>
        [TestMethod]
        [DataRow("Internal", "AbsorptionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.Systemic)]
        [DataRow("Internal", "AbsorptionFactorModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.Systemic)]
        [DataRow("Internal", "ConversionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.Internal)]
        [DataRow("Internal", "PBKModel", "AbsorptionFactorModel", InternalModelType.PBKModel, InternalModelType.ConversionFactorModel, TargetLevelType.Internal)]
        [DataRow("Internal", "ConversionFactorModel", "PBKModel", InternalModelType.ConversionFactorModel, InternalModelType.PBKModel, TargetLevelType.Internal)]
        [DataRow("Internal", "PBKModel", "ConversionFactorModel", InternalModelType.PBKModel, InternalModelType.ConversionFactorModel, TargetLevelType.Internal)]
        [DataRow("External", "AbsorptionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.External)]
        [DataRow("External", "ConversionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.External)]
        [DataRow("External", "PBKModel", "AbsorptionFactorModel", InternalModelType.PBKModel, InternalModelType.ConversionFactorModel, TargetLevelType.External)]
        public void Patch_10_01_0002_02_Test_TargetLevelType_Risks_TargetExposures(
            string targetLevelType,
            string oldTargetExposureInternalModelType,
            string oldHazardCharacterisationInternalModelType,
            InternalModelType expectedTargetExposureInternalModelType,
            InternalModelType expectedHazardCharacterisationInternalModelType,
            TargetLevelType expectedInternalExposuresTargetLevelType
        ) {
            var xmlOld = createActionXml(
                ActionType.Risks,
                "ModelledConcentration",
                targetLevelType,
                oldTargetExposureInternalModelType,
                oldHazardCharacterisationInternalModelType);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            Assert.AreEqual(expectedInternalExposuresTargetLevelType, settingsDto.TargetExposuresSettings.TargetDoseLevelType);
            Assert.AreEqual(expectedInternalExposuresTargetLevelType, settingsDto.HazardCharacterisationsSettings.TargetDoseLevelType);

            Assert.AreEqual(expectedTargetExposureInternalModelType, settingsDto.TargetExposuresSettings.InternalModelType);
            Assert.AreEqual(expectedHazardCharacterisationInternalModelType, settingsDto.HazardCharacterisationsSettings.InternalModelType);
        }

        /// <summary>
        /// Test
        /// </summary>
        [TestMethod]
        [DataRow("Internal", "AbsorptionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.Systemic)]
        [DataRow("Internal", "ConversionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.Internal)]
        [DataRow("Internal", "PBKModel", "AbsorptionFactorModel", InternalModelType.PBKModel, InternalModelType.ConversionFactorModel, TargetLevelType.Internal)]
        [DataRow("Internal", "ConversionFactorModel", "PBKModel", InternalModelType.ConversionFactorModel, InternalModelType.PBKModel, TargetLevelType.Internal)]
        [DataRow("Internal", "PBKModel", "ConversionFactorModel", InternalModelType.PBKModel, InternalModelType.ConversionFactorModel, TargetLevelType.Internal)]
        [DataRow("External", "AbsorptionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.External)]
        [DataRow("External", "ConversionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel, InternalModelType.ConversionFactorModel, TargetLevelType.External)]
        [DataRow("External", "PBKModel", "AbsorptionFactorModel", InternalModelType.PBKModel, InternalModelType.ConversionFactorModel, TargetLevelType.External)]
        public void Patch_10_01_0002_02_Test_TargetLevelType_TargetExposures(
            string targetLevelType,
            string oldTargetExposureInternalModelType,
            string oldHazardCharacterisationInternalModelType,
            InternalModelType expectedTargetExposureInternalModelType,
            InternalModelType expectedHazardCharacterisationInternalModelType,
            TargetLevelType expectedInternalExposuresTargetLevelType
        ) {
            var xmlOld = createActionXml(
                ActionType.TargetExposures,
                "MonitoringConcentration",
                targetLevelType,
                oldTargetExposureInternalModelType,
                oldHazardCharacterisationInternalModelType);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            Assert.AreEqual(expectedInternalExposuresTargetLevelType, settingsDto.TargetExposuresSettings.TargetDoseLevelType);
            Assert.AreEqual(expectedInternalExposuresTargetLevelType, settingsDto.HazardCharacterisationsSettings.TargetDoseLevelType);

            Assert.AreEqual(expectedTargetExposureInternalModelType, settingsDto.TargetExposuresSettings.InternalModelType);
            Assert.AreEqual(expectedHazardCharacterisationInternalModelType, settingsDto.HazardCharacterisationsSettings.InternalModelType);
        }

        /// <summary>
        /// Test
        /// </summary>
        [TestMethod]
        [DataRow("AbsorptionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("AbsorptionFactorModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("ConversionFactorModel", "AbsorptionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("ConversionFactorModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        [DataRow("PBKModel", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("ConversionFactorModel", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("PBKModel", "ConversionFactorModel", InternalModelType.ConversionFactorModel)]
        public void Patch_10_01_0002_02_Test_TargetLevelType_Hbm_External(
            string oldTargetExposureInternalModelType,
            string oldHazardCharacterisationInternalModelType,
            InternalModelType expectedHazardCharacterisationInternalModelType
        ) {
            var xmlOld = createActionXml(
                ActionType.HumanMonitoringAnalysis,
                "ModelledConcentration",
                "External",
                oldTargetExposureInternalModelType,
                oldHazardCharacterisationInternalModelType
            );
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            Assert.AreEqual(TargetLevelType.External, settingsDto.HazardCharacterisationsSettings.TargetDoseLevelType);
            Assert.AreEqual(expectedHazardCharacterisationInternalModelType, settingsDto.HazardCharacterisationsSettings.InternalModelType);
        }

        private string createActionXml(
            ActionType actionType,
            string exposureCalculationMethod,
            string targetLevelType,
            string targetExposureInternalModelType,
            string hazardCharacterisationInternalModelType
        ) {
            ModuleSettingsType moduleSettings = [
                ($"Risks",
                    [
                        ("ExposureCalculationMethod", exposureCalculationMethod)
                    ]
                ),
                ($"TargetExposures",
                    [
                        ("InternalModelType", targetExposureInternalModelType)
                    ]
                ),
                ($"HazardCharacterisations",
                    [
                        ("TargetDoseLevelType", targetLevelType),
                        ("InternalModelType", hazardCharacterisationInternalModelType)
                    ]
                )
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1), actionType);
            return xmlOld;
        }
    }
}
