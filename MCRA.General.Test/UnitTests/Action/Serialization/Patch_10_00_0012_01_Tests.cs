using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0012_01_Tests : ProjectSettingsSerializerTestsBase {

        /// <summary>
        /// Make options IsMcrAnalysis and McrExposureApproachType specific for the settings module Dietary, Risks, 
        /// TargetExposure and HBM.
        /// In the old version IsMcrAnalysis, ExposureApproachType and McrExposureApproachType are used;
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
            Assert.AreEqual(isMcrAnalysis, settingsDto.HumanMonitoringSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.HumanMonitoringSettings.ExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.RisksSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.RisksSettings.ExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.DietaryIntakeCalculationSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.DietaryIntakeCalculationSettings.ExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.EffectSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.EffectSettings.ExposureApproachType);
        }

        /// <summary>
        /// Make options IsMcrAnalysis and McrExposureApproachType specific for the settings module Dietary, Risks, 
        /// TargetExposure and HBM.
        /// In the old version IsMcrAnalysis, ExposureApproachType and McrExposureApproachType are used;
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
            Assert.AreEqual(isMcrAnalysis, settingsDto.HumanMonitoringSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.HumanMonitoringSettings.ExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.RisksSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.RisksSettings.ExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.DietaryIntakeCalculationSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.DietaryIntakeCalculationSettings.ExposureApproachType);
            Assert.AreEqual(isMcrAnalysis, settingsDto.EffectSettings.AnalyseMcr);
            Assert.AreEqual(approachType, settingsDto.EffectSettings.ExposureApproachType);
        }

        /// <summary>
        /// Make options IsMcrAnalysis and McrExposureApproachType specific for the settings module Dietary, Risks, 
        /// TargetExposure and HBM.
        /// In the old version IsMcrAnalysis, ExposureApproachType and McrExposureApproachType are used;
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
            Assert.IsFalse(settingsDto.HumanMonitoringSettings.AnalyseMcr);
            Assert.IsFalse(settingsDto.RisksSettings.AnalyseMcr);
            Assert.IsFalse(settingsDto.DietaryIntakeCalculationSettings.AnalyseMcr);
            Assert.IsFalse(settingsDto.EffectSettings.AnalyseMcr);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.MixtureSelectionSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.RisksSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.DietaryIntakeCalculationSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.EffectSettings.ExposureApproachType);
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
            Assert.IsTrue(settingsDto.HumanMonitoringSettings.AnalyseMcr);
            Assert.IsTrue(settingsDto.RisksSettings.AnalyseMcr);
            Assert.IsTrue(settingsDto.DietaryIntakeCalculationSettings.AnalyseMcr);
            Assert.IsTrue(settingsDto.EffectSettings.AnalyseMcr);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.MixtureSelectionSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.RisksSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.DietaryIntakeCalculationSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.EffectSettings.ExposureApproachType);
        }

        [TestMethod]
        public void Patch_10_00_0012_AddMcrOptionsEmptyLatest() {
            var xml = createMockSettingsXml(version: new(10, 0, 11));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsFalse(settingsDto.HumanMonitoringSettings.AnalyseMcr);
            Assert.IsFalse(settingsDto.RisksSettings.AnalyseMcr);
            Assert.IsFalse(settingsDto.DietaryIntakeCalculationSettings.AnalyseMcr);
            Assert.IsFalse(settingsDto.EffectSettings.AnalyseMcr);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.MixtureSelectionSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.RisksSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.DietaryIntakeCalculationSettings.ExposureApproachType);
            Assert.AreEqual(ExposureApproachType.RiskBased, settingsDto.EffectSettings.ExposureApproachType);
        }
    }
}
