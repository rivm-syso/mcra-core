using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class ProjectSettingsSerializerTests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        public void ProjectSettingsSerializer_TestDeserialize() {
            var files = new string[] {
                "MockSettings.xml",
                "Settings-Val-2887-ChronicSingleTDS.xml",
                "Settings-Val-Chronic Cumulative Pess HI.xml",
                "Settings-Val-Model-then-add.xml"
            };
            foreach (var file in files) {
                var dto = testImportSettingsXml(file, false, null, true);
                Assert.IsNotNull(dto);
            }
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestImport() {
            var dto = testImportSettingsXml("MockSettings.xml", false, null, true);
            Assert.IsNotNull(dto);
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestFixChronicPessimistic() {
            var settingsXml =
                "<AssessmentSettings>" +
                "  <AssessmentType>Exposure</AssessmentType>" +
                "  <ExposureType>Chronic</ExposureType>" +
                "</AssessmentSettings>" +
                "<ConcentrationModelSettings>" +
                "  <ConcentrationModelChoice>EfsaPessimistic</ConcentrationModelChoice>" +
                "</ConcentrationModelSettings>" +
                "<DietaryIntakeCalculationSettings>" +
                "  <DietaryIntakeCalculationTier>EfsaPessimistic</DietaryIntakeCalculationTier>" +
                "</DietaryIntakeCalculationSettings>";
            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(ActionType.DietaryExposures, settingsDto.ActionType);
            Assert.AreEqual(DietaryIntakeCalculationTier.EfsaPessimistic, settingsDto.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier);
            Assert.AreEqual(ConcentrationModelChoice.EfsaPessimistic, settingsDto.ConcentrationModelSettings.ConcentrationModelChoice);
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestSplitProcessingFactorModelTest() {
            testSplitProcessingFactorModel("None", false, false, false);
            testSplitProcessingFactorModel("Fixed", true, false, false);
            testSplitProcessingFactorModel("DistributionBased", true, true, false);
            testSplitProcessingFactorModel("DistributionBasedAllowHigher", true, true, true);
            testSplitProcessingFactorModel("FixedAllowHigher", true, false, true);
        }

        private void testSplitProcessingFactorModel(string settingValue, bool isProcessing, bool isDistribution, bool allowHigherThanOne) {
            var settingsXml =
                "<ConcentrationModelSettings>" +
                $"<ProcessingFactorModel>{settingValue}</ProcessingFactorModel>" +
                "</ConcentrationModelSettings>";
            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(isProcessing, settingsDto.ConcentrationModelSettings.IsProcessing);
            Assert.AreEqual(isDistribution, settingsDto.ConcentrationModelSettings.IsDistribution);
            Assert.AreEqual(allowHigherThanOne, settingsDto.ConcentrationModelSettings.AllowHigherThanOne);
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestSplitUnitVariabilityModelTest() {
            //NoUnitVariability, default model will be LogNormalDistribution
            testSplitUnitVariabilityModel("LogNormalDistribution", UnitVariabilityModelType.LogNormalDistribution, true);
            testSplitUnitVariabilityModel("BernoulliDistribution", UnitVariabilityModelType.BernoulliDistribution, true);
            testSplitUnitVariabilityModel("BetaDistribution", UnitVariabilityModelType.BetaDistribution, true);
            testSplitUnitVariabilityModel("NoUnitVariability", UnitVariabilityModelType.LogNormalDistribution, false);
        }

        private void testSplitUnitVariabilityModel(string settingValue, UnitVariabilityModelType modelType, bool useUnitVariability) {
            var settingsXml =
                "<UnitVariabilitySettings>" +
                $"<UnitVariabilityModel>{settingValue}</UnitVariabilityModel>" +
                "</UnitVariabilitySettings>";
            var xml = createMockSettingsXml(settingsXml);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(useUnitVariability, settingsDto.UnitVariabilitySettings.UseUnitVariability);
            Assert.AreEqual(modelType, settingsDto.UnitVariabilitySettings.UnitVariabilityModel);
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestAddOccurrenceFrequenciesSetting() {
            Func<bool, string> settingsXml = (x) =>
                "<AgriculturalUseSettings>" +
                $"<IsUseAgriculturalUseTable>{x.ToString().ToLower()}</IsUseAgriculturalUseTable>" +
                "</AgriculturalUseSettings>";
            var xml = createMockSettingsXml(settingsXml(true));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsTrue(settingsDto.AgriculturalUseSettings.UseOccurrenceFrequencies);
            Assert.IsTrue(settingsDto.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration);

            xml = createMockSettingsXml(settingsXml(false));
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsFalse(settingsDto.AgriculturalUseSettings.UseOccurrenceFrequencies);
            Assert.IsFalse(settingsDto.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration);

            //empty agriculturaluse settings, check default 'false'
            xml = createMockSettingsXml("<AgriculturalUseSettings></AgriculturalUseSettings>");
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsFalse(settingsDto.AgriculturalUseSettings.UseOccurrenceFrequencies);
            Assert.IsFalse(settingsDto.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration);

            //existing settings
            xml = createMockSettingsXml("<AgriculturalUseSettings>" +
                "<IsUseAgriculturalUseTable>true</IsUseAgriculturalUseTable>" +
                "<IsUseOccurrenceFrequencies>false</IsUseOccurrenceFrequencies>" +
                "<UseOccurrencePatternsForResidueGeneration>false</UseOccurrencePatternsForResidueGeneration>" +
                "</AgriculturalUseSettings>");
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsFalse(settingsDto.AgriculturalUseSettings.UseOccurrenceFrequencies);
            Assert.IsFalse(settingsDto.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration);

            xml = createMockSettingsXml("<AgriculturalUseSettings>" +
                "<IsUseAgriculturalUseTable>false</IsUseAgriculturalUseTable>" +
                "<IsUseOccurrenceFrequencies>true</IsUseOccurrenceFrequencies>" +
                "<UseOccurrencePatternsForResidueGeneration>true</UseOccurrencePatternsForResidueGeneration>" +
                "</AgriculturalUseSettings>");
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsTrue(settingsDto.AgriculturalUseSettings.UseOccurrenceFrequencies);
            Assert.IsTrue(settingsDto.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration);
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestRestrictCriticalEffect() {
            Func<bool, string> settingsXml = (x) =>
                "<EffectSettings>" +
                $"<RestrictToCriticalEffect>{x.ToString().ToLower()}</RestrictToCriticalEffect>" +
                "</EffectSettings>";
            var xml = createMockSettingsXml(settingsXml(true));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsTrue(settingsDto.EffectSettings.RestrictToCriticalEffect);
            Assert.IsTrue(settingsDto.EffectSettings.MultipleEffects);

            xml = createMockSettingsXml(settingsXml(false));
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsFalse(settingsDto.EffectSettings.RestrictToCriticalEffect);
            Assert.IsFalse(settingsDto.EffectSettings.MultipleEffects);

            //existing settings
            xml = createMockSettingsXml("<EffectSettings>" +
                "<RestrictToCriticalEffect>false</RestrictToCriticalEffect>" +
                "<IsMultipleEffects>false</IsMultipleEffects>" +
                "</EffectSettings>");
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsFalse(settingsDto.EffectSettings.RestrictToCriticalEffect);
            Assert.IsFalse(settingsDto.EffectSettings.MultipleEffects);

            xml = createMockSettingsXml("<EffectSettings>" +
                "<RestrictToCriticalEffect>false</RestrictToCriticalEffect>" +
                "<IsMultipleEffects>true</IsMultipleEffects>" +
                "</EffectSettings>");
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsFalse(settingsDto.EffectSettings.RestrictToCriticalEffect);
            Assert.IsTrue(settingsDto.EffectSettings.MultipleEffects);

            xml = createMockSettingsXml("<EffectSettings>" +
                "<RestrictToCriticalEffect>true</RestrictToCriticalEffect>" +
                "<IsMultipleEffects>true</IsMultipleEffects>" +
                "</EffectSettings>");
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsTrue(settingsDto.EffectSettings.RestrictToCriticalEffect);
            Assert.IsTrue(settingsDto.EffectSettings.MultipleEffects);

            xml = createMockSettingsXml("<EffectSettings>" +
                "<RestrictToCriticalEffect>true</RestrictToCriticalEffect>" +
                "<IsMultipleEffects>false</IsMultipleEffects>" +
                "</EffectSettings>");
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsTrue(settingsDto.EffectSettings.RestrictToCriticalEffect);
            Assert.IsFalse(settingsDto.EffectSettings.MultipleEffects);
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestCodeFocalEffect() {
            var codeEffect = "EffXXX";
            HashSet<string> getScope(ProjectDto dto) => dto.ScopeKeysFilters.FirstOrDefault(r => r.ScopingType == ScopingType.Effects)?.SelectedCodes ?? new HashSet<string>();

            var xml = createMockSettingsXml("<EffectSettings>" +
                "<RestrictToCriticalEffect>false</RestrictToCriticalEffect>" +
                $"<CodeEffect>{codeEffect}</CodeEffect>" +
                "</EffectSettings>");
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            var effectsScopeKeys = getScope(settingsDto);
            Assert.AreEqual(codeEffect, settingsDto.EffectSettings.CodeFocalEffect);
            CollectionAssert.AreEqual(effectsScopeKeys.ToList(), new List<string>() { codeEffect });

            xml = createMockSettingsXml(
                "<EffectSettings>" +
                $"<RestrictToCriticalEffect>true</RestrictToCriticalEffect>" +
                $"<CodeEffect>{codeEffect}</CodeEffect>" +
                "</EffectSettings>"
            );
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            effectsScopeKeys = getScope(settingsDto);
            Assert.AreEqual(codeEffect, settingsDto.EffectSettings.CodeFocalEffect);
            Assert.IsTrue(!effectsScopeKeys.Any());

            xml = createMockSettingsXml(
                "<EffectSettings>" +
                $"<CodeEffect>{codeEffect}</CodeEffect>" +
                "</EffectSettings>"
            );
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            effectsScopeKeys = getScope(settingsDto);
            Assert.AreEqual(codeEffect, settingsDto.EffectSettings.CodeFocalEffect);
            CollectionAssert.AreEqual(effectsScopeKeys.ToList(), new List<string>() { codeEffect });

            xml = createMockSettingsXml(
                "<EffectSettings>" +
                $"<CodeEffect>dontreadme</CodeEffect>" +
                $"<CodeFocalEffect>{codeEffect}</CodeFocalEffect>" +
                "</EffectSettings>"
            );
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(codeEffect, settingsDto.EffectSettings.CodeFocalEffect);

            xml = createMockSettingsXml(
                "<EffectSettings>" +
                $"<CodeEffect>dontreadme</CodeEffect>" +
                $"<CodeFocalEffect>{codeEffect}</CodeFocalEffect>" +
                "</EffectSettings>"
            );
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(codeEffect, settingsDto.EffectSettings.CodeFocalEffect);
        }

        [TestMethod]
        public void ProjectSettingsSerializer_TestPopulation() {
            var oldSettingsXml = createMockSettingsXml(version: new Version(9, 1, 32, 70));
            var newSettingsXml = createMockSettingsXml(version: new Version(9, 1, 32, 75));

            var dsConfig = new DataSourceConfiguration() {
                DataSourceMappingRecords = new List<DataSourceMappingRecord>() {
                    new DataSourceMappingRecord() {
                        SourceTableGroup = SourceTableGroup.Survey
                    }
                }
            };

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(oldSettingsXml, null, false, out _);
            Assert.IsTrue(settingsDto.CalculationActionTypes.Contains(ActionType.Populations));

            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(oldSettingsXml, dsConfig, false, out _);
            Assert.IsTrue(settingsDto.CalculationActionTypes.Contains(ActionType.Populations));

            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(newSettingsXml, dsConfig, false, out _);
            Assert.IsFalse(settingsDto.CalculationActionTypes.Contains(ActionType.Populations));
        }
    }
}
