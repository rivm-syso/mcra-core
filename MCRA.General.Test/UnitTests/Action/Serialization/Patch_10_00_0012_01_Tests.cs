using MCRA.General.Action.Serialization;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0012_01_Tests : ProjectSettingsSerializerTestsBase {

        /// <summary>
        /// Make options IsMcrAnalysis and McrExposureApproachType specific for the settings module Dietary, Risks,
        /// TargetExposure and HBM.
        /// In the old version IsMcrAnalysis, McrExposureApproachType and McrExposureApproachType are used;
        /// The values of IsMcrAnalysis and McrExposureApproachType should be used for the new options in the modules
        /// Test settings new projects.
        /// </summary>
        [TestMethod]
        [DataRow(true, ExposureApproachType.RiskBased)]
        [DataRow(false, ExposureApproachType.RiskBased)]
        [DataRow(true, ExposureApproachType.ExposureBased)]
        [DataRow(false, ExposureApproachType.ExposureBased)]
        [DataRow(true, ExposureApproachType.UnweightedExposures)]
        [DataRow(false, ExposureApproachType.UnweightedExposures)]
        public void Patch_10_00_0012_AddMcrOptionsHbmNewVersion(
            bool isMcrAnalysis,
            ExposureApproachType approachType
        ) {
            var settingsXml =
                "<MixtureSelectionSettings>" +
                $"  <IsMcrAnalysis>{(isMcrAnalysis ? "true" : "false")}</IsMcrAnalysis>" +
                $"  <McrExposureApproachType>{approachType}</McrExposureApproachType>" +
                "</MixtureSelectionSettings>";
            var xml = createMockSettingsXml(settingsXml, new(10, 0, 11));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(isMcrAnalysis, settingsDto.DietaryExposuresSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.DietaryExposuresSettings.McrExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.HumanMonitoringAnalysisSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.HumanMonitoringAnalysisSettings.McrExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.RisksSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.RisksSettings.McrExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.TargetExposuresSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.TargetExposuresSettings.McrExposureApproachType);
        }

        /// <summary>
        /// Make options IsMcrAnalysis and McrExposureApproachType specific for the settings module Dietary, Risks,
        /// TargetExposure and HBM.
        /// In the old version IsMcrAnalysis, McrExposureApproachType and McrExposureApproachType are used;
        /// The values of IsMcrAnalysis and McrExposureApproachType should be used for the new options in the modules
        /// Test settings old projects.
        [TestMethod]
        [DataRow(true, ExposureApproachType.RiskBased)]
        [DataRow(false, ExposureApproachType.RiskBased)]
        [DataRow(true, ExposureApproachType.ExposureBased)]
        [DataRow(false, ExposureApproachType.ExposureBased)]
        [DataRow(true, ExposureApproachType.UnweightedExposures)]
        [DataRow(false, ExposureApproachType.UnweightedExposures)]
        public void Patch_10_00_0012_AddMcrOptionsOldVersion(
            bool isMcrAnalysis,
            ExposureApproachType approachType
        ) {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<DietaryIntakeCalculationSettings></DietaryIntakeCalculationSettings>" +
                "<RisksSettings></RisksSettings>" +
                "<EffectSettings></EffectSettings>" +
                "<MixtureSelectionSettings>" +
                $"  <IsMcrAnalysis>{(isMcrAnalysis ? "true" : "false")}</IsMcrAnalysis>" +
                $"  <McrExposureApproachType>{approachType}</McrExposureApproachType>" +
                "</MixtureSelectionSettings>";
            var xml = createMockSettingsXml(settingsXml, new(10, 0, 11));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(isMcrAnalysis, settingsDto.DietaryExposuresSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.DietaryExposuresSettings.McrExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.HumanMonitoringAnalysisSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.HumanMonitoringAnalysisSettings.McrExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.RisksSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.RisksSettings.McrExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.TargetExposuresSettings.McrAnalysis);
            Assert.AreEqual(approachType, settingsDto.TargetExposuresSettings.McrExposureApproachType);
        }

        /// <summary>
        /// Make options IsMcrAnalysis and McrExposureApproachType specific for the settings module Dietary, Risks,
        /// TargetExposure and HBM.
        /// In the old version IsMcrAnalysis, McrExposureApproachType and McrExposureApproachType are used;
        /// The values of IsMcrAnalysis and McrExposureApproachType should be used for the new options in the modules
        /// Test settings old projects.
        [TestMethod]
        public void Patch_10_00_0012_AddMcrOptionsEmptyElements() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<DietaryIntakeCalculationSettings></DietaryIntakeCalculationSettings>" +
                "<RisksSettings></RisksSettings>" +
                "<EffectSettings></EffectSettings>";
            var xml = createMockSettingsXml(settingsXml, new(10, 0, 11));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsFalse(settingsDto.DietaryExposuresSettings.McrAnalysis);
            Assert.IsFalse(settingsDto.HumanMonitoringAnalysisSettings.McrAnalysis);
            Assert.IsFalse(settingsDto.RisksSettings.McrAnalysis);
            Assert.IsFalse(settingsDto.TargetExposuresSettings.McrAnalysis);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.DietaryExposuresSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.HumanMonitoringAnalysisSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.RisksSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.TargetExposuresSettings.McrExposureApproachType);
        }

        /// <summary>
        /// Apply all patches (no version given so assumes oldest)
        /// Patch 9.02.0007 sets a default of 'true' for IsMcrAnalysis
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0012_AddMcrOptionsEmptyAllPatches() {
            var xml = createMockSettingsXml();
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsTrue(settingsDto.DietaryExposuresSettings.McrAnalysis);
            Assert.IsTrue(settingsDto.HumanMonitoringAnalysisSettings.McrAnalysis);
            Assert.IsTrue(settingsDto.RisksSettings.McrAnalysis);
            Assert.IsTrue(settingsDto.TargetExposuresSettings.McrAnalysis);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.DietaryExposuresSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.HumanMonitoringAnalysisSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.RisksSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.TargetExposuresSettings.McrExposureApproachType);
        }

        [TestMethod]
        public void Patch_10_00_0012_AddMcrOptionsEmptyLatest() {
            var xml = createMockSettingsXml(version: new(10, 0, 11));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsFalse(settingsDto.DietaryExposuresSettings.McrAnalysis);
            Assert.IsFalse(settingsDto.HumanMonitoringAnalysisSettings.McrAnalysis);
            Assert.IsFalse(settingsDto.RisksSettings.McrAnalysis);
            Assert.IsFalse(settingsDto.TargetExposuresSettings.McrAnalysis);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.DietaryExposuresSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.HumanMonitoringAnalysisSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.RisksSettings.McrExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.TargetExposuresSettings.McrExposureApproachType);
        }
    }
}
