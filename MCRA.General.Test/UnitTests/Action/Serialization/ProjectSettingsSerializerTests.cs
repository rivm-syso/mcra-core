using System.Text;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class ProjectSettingsSerializerTests {

        private static readonly string _outputPath =
            Path.Combine(TestResourceUtilities.OutputResourcesPath, "Serialization");

        private static readonly string _xmlResourcesPath = @"Resources\Xml\";

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
        public void ProjectSettingsSerializer_TestMatchIndividualSubsetWithPopulation() {
            Func<bool, bool, bool, string> settingsXml = (x1, x2, x3) =>
                "<SubsetSettings>" +
                $"<PopulationSubsetSelection>true</PopulationSubsetSelection>" +
                $"<MatchIndividualsWithPopulation>{x1.ToString().ToLower()}</MatchIndividualsWithPopulation>" +
                $"<MatchIndividualDaysWithPopulation>{x2.ToString().ToLower()}</MatchIndividualDaysWithPopulation> " +
                $"<IndividualDaySubsetSelection>{x3.ToString().ToLower()}</IndividualDaySubsetSelection>" +
                "</SubsetSettings>";

            var xml = createMockSettingsXml(settingsXml(true, false, false), version: new Version(9, 1, 37));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(IndividualSubsetType.MatchToPopulationDefinition, settingsDto.SubsetSettings.MatchIndividualSubsetWithPopulation);
            Assert.IsTrue(settingsDto.SubsetSettings.PopulationSubsetSelection);

            xml = createMockSettingsXml(settingsXml(false, true, false), version: new Version(9, 1, 37));
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(IndividualSubsetType.IgnorePopulationDefinition, settingsDto.SubsetSettings.MatchIndividualSubsetWithPopulation);
            Assert.IsTrue(settingsDto.SubsetSettings.PopulationSubsetSelection);
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

        [TestMethod]
        [DataRow("CosmosV4", "EuroMix_Generic_PBTK_model_V5")]
        [DataRow("CosmosV6", "EuroMix_Generic_PBTK_model_V6")]
        [DataRow("PBPKModel_BPA", "EuroMix_Bisphenols_PBPK_model_V1")]
        [DataRow("PBPKModel_BPA_Reimplementation", "EuroMix_Bisphenols_PBPK_model_V2")]
        [DataRow("XXX", "XXX")]
        public void ProjectSettingsSerializer_TestRecodeKineticModels_v9_1_46(string oldCode, string newCode) {
            Func<string, string> createSettingsXml = (code) =>
                "<KineticModelSettings>" +
                $"<CodeModel>{code}</CodeModel>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(createSettingsXml(oldCode));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(newCode, settingsDto.KineticModelSettings.CodeModel);
        }

        [TestMethod]
        [DataRow("true", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("false", "PBKModel", InternalModelType.PBKModel)]
        [DataRow("true", "AbsorptionFactorModel", InternalModelType.AbsorptionFactorModel)]
        [DataRow("false", "AbsorptionFactorModel", InternalModelType.AbsorptionFactorModel)]
        [DataRow("true", null, InternalModelType.PBKModel)]
        [DataRow("false", null, InternalModelType.AbsorptionFactorModel)]
        public void ProjectSettingsSerializer_TestRemoveUseKineticModel_v9_1_47(
            string useKineticModel, 
            string internalModelType,
            InternalModelType expectedModelType
        ) {
            Func<string, string, string> createSettingsXml = (use, model) =>
                "<KineticModelSettings>" +
                $"<UseKineticModel>{use}</UseKineticModel>" +
                (!string.IsNullOrEmpty(model) ? $"<InternalModelType>{model}</InternalModelType>" : string.Empty) +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(createSettingsXml(useKineticModel, internalModelType));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);

            Assert.AreEqual(expectedModelType, settingsDto.KineticModelSettings.InternalModelType);
        }

        [TestMethod]
        [DataRow(InternalConcentrationType.ModelledConcentration)]
        [DataRow(InternalConcentrationType.MonitoringConcentration)]
        public void ProjectSettingsSerializer_MoveInternalConcentrationTypeToAssessmentSettings_v9_2_04(InternalConcentrationType internalConcentrationType) {
            Func<InternalConcentrationType, string> createSettingsXml = (internalConcentrationType) =>
                "<AssessmentSettings>" +
                "</AssessmentSettings>" +
                "<MixtureSelectionSettings>" +
                $"<InternalConcentrationType>{internalConcentrationType}</InternalConcentrationType>" +
                "</MixtureSelectionSettings>";
            var xml = createMockSettingsXml(createSettingsXml(internalConcentrationType));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.AreEqual(internalConcentrationType, settingsDto.AssessmentSettings.InternalConcentrationType);
        }

        #region Helpers

        private static ProjectDto testImportSettingsXml(
            string filename,
            bool isOldStyle,
            DataSourceConfiguration dataSourceConfiguration,
            bool writeOutput = true
        ) {
            var settings = ProjectSettingsSerializer.ImportFromXmlFile($"{_xmlResourcesPath}{filename}", dataSourceConfiguration, isOldStyle, out var modified);
            if (writeOutput) {
                writeProjectSettingsXml(filename, settings);
            }
            return settings;
        }

        private static void writeProjectSettingsXml(string filename, ProjectDto project) {
            if (!Directory.Exists(_outputPath)) {
                Directory.CreateDirectory(_outputPath);
            }
            var outputFile = Path.Combine(_outputPath, $"Transformed{filename}");
            ProjectSettingsSerializer.ExportToXmlFile(project, outputFile, true);
        }

        private static string createMockSettingsXml(string settingsXml = null, Version version = null) {
            var sb = new StringBuilder();
            sb.Append("<Project xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\">");
            if (version != null) {
                sb.Append(
                    $"<McraVersion>" +
                    $"<Major>{version.Major}</Major>" +
                    $"<Minor>{version.Minor}</Minor>" +
                    $"<Build>{version.Build}</Build>" +
                    $"<Revision>{version.Revision}</Revision>" +
                    $"</McraVersion>");
            }
            if (!string.IsNullOrEmpty(settingsXml)) {
                sb.Append(settingsXml);
            }
            sb.Append("</Project>");
            return sb.ToString();
        }

        #endregion
    }
}
