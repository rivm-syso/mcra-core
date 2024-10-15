using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0002_00_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        [DataRow(ActionType.Risks, "Internal", "External", TargetLevelType.External)]
        [DataRow(ActionType.Risks, "External", "Internal", TargetLevelType.Internal)]
        [DataRow(ActionType.Unknown, "Internal", "External", TargetLevelType.External)]
        [DataRow(ActionType.Unknown, "External", "Internal", TargetLevelType.Internal)]
        [DataRow(ActionType.HumanMonitoringAnalysis, "Internal", "External", TargetLevelType.Internal)]
        [DataRow(ActionType.HumanMonitoringAnalysis, "External", "Internal", TargetLevelType.External)]
        public void Patch_10_01_0002_00_Tests_TestHbmTargetSurfaceLevel_hbm(
            ActionType actiontype,
            string hbmTargetSurfaceLevel,
            string hcTargetDoseLevelType,
            TargetLevelType expectedTargetLevelType
        ) {
            ModuleSettingsType moduleSettings = [
                ("HumanMonitoringAnalysis", [
                    ("HbmTargetSurfaceLevel", hbmTargetSurfaceLevel)
                ]),
                ("HazardCharacterisations", [
                    ("TargetDoseLevelType", hcTargetDoseLevelType)
                ])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1), actiontype);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.HumanMonitoringAnalysisSettings;

            Assert.IsNotNull(modSettings);
            Assert.AreEqual(expectedTargetLevelType, modSettings.TargetDoseLevelType);
        }

        [TestMethod]
        [DataRow(ActionType.Risks, TargetLevelType.External)]
        [DataRow(ActionType.HumanMonitoringAnalysis, TargetLevelType.Internal)]
        public void Patch_10_01_0002_00_Tests_TestHbmTargetSurfaceLevel_NoHcSettings(
            ActionType actiontype,
            TargetLevelType expectedTargetLevelType
        ) {
            ModuleSettingsType moduleSettings = [
                ("HumanMonitoringAnalysis", [
                    ("HbmTargetSurfaceLevel", "Internal")
                ])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1), actiontype);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            var modSettings = settingsDto.HumanMonitoringAnalysisSettings;

            Assert.IsNotNull(modSettings);
            Assert.AreEqual(expectedTargetLevelType, modSettings.TargetDoseLevelType);
        }
    }
}
