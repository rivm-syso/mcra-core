using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0000_Tests : ProjectSettingsSerializerTestsBase {
        /// <summary>
        /// Test patch 10.00.0000
        /// Id of EFSA 2022 Dietary CRA tier was split out into two separate tiers for chronic and acute
        /// during development. Convert module tier settings of actions using the original tiers to the
        /// acute version of the new tiers. This test is for tier I settings.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0000_TestRenameEfsa2022DietaryCraTier1() {
            var settingsXml =
                "<ConcentrationModelSettings>" +
                "  <ConcentrationModelChoice>Efsa2022DietaryCraTier1</ConcentrationModelChoice>" +
                "  <ConcentrationsTier>Efsa2022DietaryCraTier1</ConcentrationsTier>" +
                "</ConcentrationModelSettings>" +
                "<AgriculturalUseSettings>" +
                "  <OccurrencePatternsTier>Efsa2022DietaryCraTier1</OccurrencePatternsTier>" +
                "</AgriculturalUseSettings>" +
                "<FoodSurveySettings>" +
                "  <ConsumptionsTier>Efsa2022DietaryCraTier1</ConsumptionsTier>" +
                "</FoodSurveySettings>" +
                "<EffectModelSettings>" +
                "  <RiskCalculationTier>Efsa2022DietaryCraTier1</RiskCalculationTier>" +
                "  <SingleValueRisksCalculationTier>Efsa2022DietaryCraTier1</SingleValueRisksCalculationTier>" +
                "</EffectModelSettings>";
            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.EffectModelSettings.RiskCalculationTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.EffectModelSettings.SingleValueRisksCalculationTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.ConcentrationModelSettings.ConcentrationsTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.ConcentrationModelSettings.ConcentrationModelChoice);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.AgriculturalUseSettings.OccurrencePatternsTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.FoodSurveySettings.ConsumptionsTier);
        }

        /// <summary>
        /// Test patch 10.00.0000
        /// Id of EFSA 2022 Dietary CRA tier was split out into two separate tiers for chronic and acute
        /// during development. Convert module tier settings of actions using the original tiers to the
        /// acute version of the new tiers. This test is for tier I settings.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0000_TestRenameEfsa2022DietaryCraTier2() {
            var settingsXml =
                "<ConcentrationModelSettings>" +
                "  <ConcentrationModelChoice>Efsa2022DietaryCraTier2</ConcentrationModelChoice>" +
                "  <ConcentrationsTier>Efsa2022DietaryCraTier2</ConcentrationsTier>" +
                "</ConcentrationModelSettings>" +
                "<AgriculturalUseSettings>" +
                "  <OccurrencePatternsTier>Efsa2022DietaryCraTier2</OccurrencePatternsTier>" +
                "</AgriculturalUseSettings>" +
                "<FoodSurveySettings>" +
                "  <ConsumptionsTier>Efsa2022DietaryCraTier2</ConsumptionsTier>" +
                "</FoodSurveySettings>" +
                "<EffectModelSettings>" +
                "  <RiskCalculationTier>Efsa2022DietaryCraTier2</RiskCalculationTier>" +
                "  <SingleValueRisksCalculationTier>Efsa2022DietaryCraTier2</SingleValueRisksCalculationTier>" +
                "</EffectModelSettings>";
            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.EffectModelSettings.RiskCalculationTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.EffectModelSettings.SingleValueRisksCalculationTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.ConcentrationModelSettings.ConcentrationsTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.ConcentrationModelSettings.ConcentrationModelChoice);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.AgriculturalUseSettings.OccurrencePatternsTier);
            Assert.AreEqual(SettingsTemplateType.Custom, settingsDto.FoodSurveySettings.ConsumptionsTier);
        }
    }
}
