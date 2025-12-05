using System.Xml;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0001_Tests : ProjectSettingsSerializerTestsBase {
        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix1() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>Blood</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Blood,
                settingsDto.HumanMonitoringAnalysisSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix2() {
            var settingsXml =
                "<KineticModelSettings>" +
                "  <CodeCompartment>Blood</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Undefined,
                settingsDto.HumanMonitoringAnalysisSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrixNonsense() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>NonsensicalEnumValue</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Undefined,
                settingsDto.HumanMonitoringAnalysisSettings.TargetMatrix
            );
        }


        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix3() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>CLiver</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Liver,
                settingsDto.HumanMonitoringAnalysisSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix4() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>Aaaaaaaaaa</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Undefined,
                settingsDto.HumanMonitoringAnalysisSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrixBloodSerum() {
            var settingsXml =
                "<HumanMonitoringSettings>" +
                "  <SamplingMethodCodes>" +
                "    <string>Blood_Serum</string>" +
                "  </SamplingMethodCodes>" +
                "</HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>Blood</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.BloodSerum,
                settingsDto.HumanMonitoringAnalysisSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrixUrine() {
            var settingsXml =
                "<HumanMonitoringSettings>" +
                "  <SamplingMethodCodes>" +
                "    <string>Urine_Spot</string>" +
                "    <string>Blood_Serum</string>" +
                "  </SamplingMethodCodes>" +
                "</HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>Urine</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Urine,
                settingsDto.HumanMonitoringAnalysisSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmSamplingMethodCodesWithNewBiologicalMatrixNames() {
            var settingsXml =
                "<HumanMonitoringSettings>" +
                "  <SamplingMethodCodes>" +
                "    <string>Urine_Spot</string>" +
                "    <string>Blood_Serum</string>" +
                "  </SamplingMethodCodes>" +
                "</HumanMonitoringSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.Contains("BloodSerum_Serum", settingsDto.HumanMonitoringDataSettings.CodesHumanMonitoringSamplingMethods);
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// Remove KineticModelSettings/NumberOfIndividuals
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestRemoveKineticModelsNumberOfIndividuals() {
            var settingsXml =
                "<KineticModelSettings>" +
                "  <NumberOfIndividuals>100</NumberOfIndividuals>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var node = getXmlNode(xml, "//Project//KineticModelSettings//NumberOfIndividuals");
            Assert.IsNotNull(node);

            var patchedXml = applyPatch(xml, "Patch-10.00.0001-00.xslt");

            static XmlNode getXmlNode(string patchedXml, string path) {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(patchedXml);
                var root = xmlDoc.DocumentElement;
                var result = root.SelectSingleNode(path);
                return result;
            }

            node = getXmlNode(patchedXml, "/Project/KineticModelSettings/NumberOfIndividuals");
            Assert.IsNull(node);
        }


        /// <summary>
        /// Test patch 10.00.0001.
        /// Replace single CodeFood containing DTO classes with simple string lists
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestCodeFoodReplacements() {
            var settingsXml =
                "<FoodAsEatenSubset>" +
                    "<FoodAsEatenSubsetDto><CodeFood>BANANA</CodeFood></FoodAsEatenSubsetDto>" +
                    "<FoodAsEatenSubsetDto><CodeFood>PINEAPPLE</CodeFood></FoodAsEatenSubsetDto>" +
                "</FoodAsEatenSubset>" +
                "<ModelledFoodSubset>" +
                    "<ModelledFoodSubsetDto><CodeFood>Milk duds</CodeFood></ModelledFoodSubsetDto>" +
                    "<ModelledFoodSubsetDto><CodeFood>Apple pie</CodeFood></ModelledFoodSubsetDto>" +
                    "<ModelledFoodSubsetDto><CodeFood>PIZZA</CodeFood></ModelledFoodSubsetDto>" +
                "</ModelledFoodSubset>" +
                "<SelectedScenarioAnalysisFoods>" +
                    "<SelectedScenarioAnalysisFoodDto><CodeFood>Orange</CodeFood></SelectedScenarioAnalysisFoodDto>" +
                    "<SelectedScenarioAnalysisFoodDto><CodeFood>Tomato</CodeFood></SelectedScenarioAnalysisFoodDto>" +
                    "<SelectedScenarioAnalysisFoodDto><CodeFood>Minneola</CodeFood></SelectedScenarioAnalysisFoodDto>" +
                "</SelectedScenarioAnalysisFoods>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual("BANANA,PINEAPPLE", string.Join(",", settingsDto.ConsumptionsSettings.FoodAsEatenSubset));
            Assert.AreEqual("Milk duds,Apple pie,PIZZA", string.Join(",", settingsDto.ModelledFoodsSettings.ModelledFoodSubset));
            Assert.AreEqual("Orange,Tomato,Minneola", string.Join(",", settingsDto.DietaryExposuresSettings.ScenarioAnalysisFoods));
        }


        /// <summary>
        /// Test patch 10.00.0001.
        /// Replace single CodeFood containing DTO classes with simple string lists
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestCodeFoodReplacementIntakeModelPerCategory() {
            var settingsXml =
                "<IntakeModelSettings>" +
                "<IntakeModelsPerCategory>" +
                    "<IntakeModelPerCategoryDto>" +
                        "<FoodsAsMeasured>" +
                            "<IntakeModelPerCategory_FoodAsMeasuredDto><CodeFood>BANANA</CodeFood></IntakeModelPerCategory_FoodAsMeasuredDto>" +
                            "<IntakeModelPerCategory_FoodAsMeasuredDto><CodeFood>PINEAPPLE</CodeFood></IntakeModelPerCategory_FoodAsMeasuredDto>" +
                        "</FoodsAsMeasured>" +
                    "</IntakeModelPerCategoryDto>" +
                    "<IntakeModelPerCategoryDto>" +
                        "<FoodsAsMeasured>" +
                            "<IntakeModelPerCategory_FoodAsMeasuredDto><CodeFood>Potatoes</CodeFood></IntakeModelPerCategory_FoodAsMeasuredDto>" +
                            "<IntakeModelPerCategory_FoodAsMeasuredDto><CodeFood>Mushrooms</CodeFood></IntakeModelPerCategory_FoodAsMeasuredDto>" +
                        "</FoodsAsMeasured>" +
                    "</IntakeModelPerCategoryDto>" +
                "</IntakeModelsPerCategory>" +
                "</IntakeModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            var config = settingsDto.DietaryExposuresSettings;
            Assert.AreEqual("BANANA,PINEAPPLE,Potatoes,Mushrooms", string.Join(",", config.IntakeModelsPerCategory.SelectMany(r => r.FoodsAsMeasured)));
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Patch_10_00_0001_TestRenameImputeHbmConcentrationsFromOtherMatrices_TestSettingExists(
            bool value
        ) {
            var settingsXml =
                "<EffectSettings>" +
                $"  <HazardCharacterisationsConvertToSingleTargetMatrix>{value.ToString().ToLower()}</HazardCharacterisationsConvertToSingleTargetMatrix>" +
                "</EffectSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(value, settingsDto.HazardCharacterisationsSettings.HazardCharacterisationsConvertToSingleTargetMatrix);
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestRenameImputeHbmConcentrationsFromOtherMatrices_TestDefaultIfNew() {
            var settingsXml =
                "<EffectSettings>" +
                "<AdditionalAssessmentFactor>20</AdditionalAssessmentFactor>" +
                "</EffectSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            var config= settingsDto.HazardCharacterisationsSettings;
            Assert.AreEqual(20, config.AdditionalAssessmentFactor);
            Assert.IsTrue(config.HazardCharacterisationsConvertToSingleTargetMatrix);
        }


        /// <summary>
        /// Test patch 10.00.0001
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_01_EffectModelSettings() {
            var settingsXml =
            "<EffectModelSettings>" +
                "<RiskCalculationTier>Efsa2022DietaryCraChronicTier1</RiskCalculationTier>" +
                "<SingleValueRisksCalculationTier>Efsa2022DietaryCraAcuteTier2</SingleValueRisksCalculationTier>" +
                "<HealthEffectType>Benefit</HealthEffectType>" +
                "<LeftMargin>1.23456</LeftMargin>" +
                "<RightMargin>2.34567</RightMargin>" +
                "<IsEAD>true</IsEAD>" +
                "<ThresholdMarginOfExposure>3.45678</ThresholdMarginOfExposure>" +
                "<ConfidenceInterval>4.56789</ConfidenceInterval>" +
                "<DefaultInterSpeciesFactorGeometricMean>5.67891</DefaultInterSpeciesFactorGeometricMean>" +
                "<DefaultInterSpeciesFactorGeometricStandardDeviation>6.78912</DefaultInterSpeciesFactorGeometricStandardDeviation>" +
                "<DefaultIntraSpeciesFactor>7.89123</DefaultIntraSpeciesFactor>" +
                "<NumberOfLabels>888</NumberOfLabels>" +
                "<CumulativeRisk>true</CumulativeRisk>" +
                "<RiskMetricType>HazardIndex</RiskMetricType>" +
                "<RiskMetricCalculationType>SumRatios</RiskMetricCalculationType>" +
                "<NumberOfSubstances>123</NumberOfSubstances>" +
                "<SingleValueRiskCalculationMethod>FromIndividualRisks</SingleValueRiskCalculationMethod>" +
                "<Percentage>67.89123</Percentage>" +
                "<IsInverseDistribution>true</IsInverseDistribution>" +
                "<UseAdjustmentFactors>true</UseAdjustmentFactors>" +
                "<ExposureAdjustmentFactorDistributionMethod>Gamma</ExposureAdjustmentFactorDistributionMethod>" +
                "<HazardAdjustmentFactorDistributionMethod>LogNormal</HazardAdjustmentFactorDistributionMethod>" +
                "<ExposureParameterA>9</ExposureParameterA>" +
                "<ExposureParameterB>10</ExposureParameterB>" +
                "<ExposureParameterC>11</ExposureParameterC>" +
                "<ExposureParameterD>12</ExposureParameterD>" +
                "<HazardParameterA>13</HazardParameterA>" +
                "<HazardParameterB>14</HazardParameterB>" +
                "<HazardParameterC>15</HazardParameterC>" +
                "<HazardParameterD>16</HazardParameterD>" +
                "<UseBackgroundAdjustmentFactor>true</UseBackgroundAdjustmentFactor>" +
                "<CalculateRisksByFood>true</CalculateRisksByFood>" +
            "</EffectModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);
            var config = settings.RisksSettings;
            var svrConfig = settings.SingleValueRisksSettings;

            Assert.AreEqual(SettingsTemplateType.Efsa2022DietaryCraAcuteTier2, config.SelectedTier);
            Assert.AreEqual(SettingsTemplateType.Efsa2022DietaryCraAcuteTier2, svrConfig.SelectedTier);
            Assert.AreEqual(HealthEffectType.Benefit, config.HealthEffectType);
            Assert.AreEqual(1.23456, config.LeftMargin);
            Assert.AreEqual(2.34567, config.RightMargin);
            Assert.IsTrue(config.IsEAD);
            Assert.AreEqual(3.45678, config.ThresholdMarginOfExposure);
            Assert.AreEqual(4.56789, config.ConfidenceInterval);
            Assert.AreEqual(5.67891, settings.InterSpeciesConversionsSettings.DefaultInterSpeciesFactorGeometricMean);
            Assert.AreEqual(6.78912, settings.InterSpeciesConversionsSettings.DefaultInterSpeciesFactorGeometricStandardDeviation);
            Assert.AreEqual(7.89123, settings.IntraSpeciesFactorsSettings.DefaultIntraSpeciesFactor);
            Assert.AreEqual(888, config.NumberOfLabels);
            Assert.IsTrue(config.CumulativeRisk);
            Assert.AreEqual(RiskMetricType.ExposureHazardRatio, config.RiskMetricType);
            Assert.AreEqual(RiskMetricCalculationType.SumRatios, config.RiskMetricCalculationType);
            Assert.AreEqual(123, config.NumberOfSubstances);
            Assert.AreEqual(SingleValueRiskCalculationMethod.FromIndividualRisks, svrConfig.SingleValueRiskCalculationMethod);
            Assert.AreEqual(67.89123, svrConfig.Percentage);
            Assert.IsTrue(config.IsInverseDistribution);
            Assert.IsTrue(svrConfig.UseAdjustmentFactors);
            Assert.AreEqual(AdjustmentFactorDistributionMethod.Gamma, svrConfig.ExposureAdjustmentFactorDistributionMethod);
            Assert.AreEqual(AdjustmentFactorDistributionMethod.LogNormal, svrConfig.HazardAdjustmentFactorDistributionMethod);
            Assert.AreEqual(9, svrConfig.ExposureParameterA);
            Assert.AreEqual(10, svrConfig.ExposureParameterB);
            Assert.AreEqual(11, svrConfig.ExposureParameterC);
            Assert.AreEqual(12, svrConfig.ExposureParameterD);
            Assert.AreEqual(13, svrConfig.HazardParameterA);
            Assert.AreEqual(14, svrConfig.HazardParameterB);
            Assert.AreEqual(15, svrConfig.HazardParameterC);
            Assert.AreEqual(16, svrConfig.HazardParameterD);
            Assert.IsTrue(svrConfig.UseBackgroundAdjustmentFactor);
            Assert.IsTrue(config.CalculateRisksByFood);
        }

        /// <summary>
        /// Test patch 10.00.0001
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_50_IntakeModelSettings() {
            var settingsXml =
            "<IntakeModelSettings>" +
                "<IntakeModelType>BBN</IntakeModelType>" +
                "<TransformType>Power</TransformType>" +
                "<GridPrecision>456</GridPrecision>" +
                "<NumberOfIterations>789</NumberOfIterations>" +
                "<SplineFit>true</SplineFit>" +
                "<CovariateModelling>true</CovariateModelling>" +
                "<FirstModelThenAdd>true</FirstModelThenAdd>" +
                "<Dispersion>0.12345</Dispersion>" +
                "<VarianceRatio>1.23456</VarianceRatio>" +
                "<IntakeModelsPerCategory>" +
                "  <IntakeModelPerCategoryDto>" +
                "    <ModelType>LNN0</ModelType>" +
                "    <TransformType>NoTransform</TransformType>" +
                "    <FoodsAsMeasured>" +
                "      <FoodCode>BANANA</FoodCode>" +
                "      <FoodCode>Pear</FoodCode>" +
                "    </FoodsAsMeasured>" +
                "  </IntakeModelPerCategoryDto>" +
                "  <IntakeModelPerCategoryDto>" +
                "    <ModelType>ISUF</ModelType>" +
                "    <TransformType>Power</TransformType>" +
                "    <FoodsAsMeasured>" +
                "      <FoodCode>Apple</FoodCode>" +
                "    </FoodsAsMeasured>" +
                "  </IntakeModelPerCategoryDto>" +
                "</IntakeModelsPerCategory>" +
            "</IntakeModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);
            var config = settings.DietaryExposuresSettings;
            Assert.AreEqual(IntakeModelType.BBN, config.IntakeModelType);
            Assert.AreEqual(TransformType.Power, config.AmountModelTransformType);
            Assert.AreEqual(456, config.IsufModelGridPrecision);
            Assert.AreEqual(789, config.IsufModelNumberOfIterations);
            Assert.IsTrue(config.IsufModelSplineFit);
            Assert.IsTrue(config.IntakeCovariateModelling);
            Assert.IsTrue(config.IntakeFirstModelThenAdd);
            Assert.AreEqual(0.12345, config.FrequencyModelDispersion);
            Assert.AreEqual(1.23456, config.AmountModelVarianceRatio);
            var models = config.IntakeModelsPerCategory;
            Assert.IsNotNull(models);
            Assert.HasCount(2, models);
            Assert.AreEqual(IntakeModelType.LNN0, models[0].ModelType);
            Assert.AreEqual(TransformType.NoTransform, models[0].TransformType);
            Assert.AreEqual("BANANA,Pear", string.Join(",", models[0].FoodsAsMeasured));
            Assert.AreEqual(IntakeModelType.ISUF, models[1].ModelType);
            Assert.AreEqual(TransformType.Power, models[1].TransformType);
            Assert.AreEqual("Apple", string.Join(",", models[1].FoodsAsMeasured));
        }

        /// <summary>
        /// Test patch 10.00.0001
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_50_ProjectIndividualsSubsetDefinitions() {
            var settingsXml =
            "<IndividualsSubsetDefinitions>" +
                "<IndividualsSubsetDefinitionDto>" +
                    "<NameIndividualProperty>Age</NameIndividualProperty>" +
                    "<IndividualPropertyQuery>45-76</IndividualPropertyQuery>" +
                "</IndividualsSubsetDefinitionDto>" +
                "<IndividualsSubsetDefinitionDto>" +
                    "<NameIndividualProperty>Gender</NameIndividualProperty>" +
                    "<IndividualPropertyQuery>'F','M'</IndividualPropertyQuery>" +
                "</IndividualsSubsetDefinitionDto>" +
            "</IndividualsSubsetDefinitions>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);
            var config = settings.ConsumptionsSettings;
            var individualsDefs = config.IndividualsSubsetDefinitions;
            Assert.HasCount(2, individualsDefs);
            Assert.AreEqual("Age", individualsDefs[0].NameIndividualProperty);
            Assert.AreEqual("45-76", individualsDefs[0].IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.Range, individualsDefs[0].GetQueryDefinitionType());
            Assert.AreEqual("Gender", individualsDefs[1].NameIndividualProperty);
            Assert.AreEqual("'F','M'", individualsDefs[1].IndividualPropertyQuery);
            Assert.AreEqual("F M", string.Join(" ", individualsDefs[1].GetQueryKeywords()));
            Assert.AreEqual(QueryDefinitionType.ValueList, individualsDefs[1].GetQueryDefinitionType());
        }

        /// <summary>
        /// Test patch 10.00.0001
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_50_ProjectSamplesSubsetDefinitions() {
            var settingsXml =
            "<SamplesSubsetDefinitions>" +
                "<SamplesSubsetDefinitionDto>" +
                    "<PropertyName>Proper1</PropertyName>" +
                    "<AlignSubsetWithPopulation>true</AlignSubsetWithPopulation>" +
                    "<IncludeMissingValueRecords>true</IncludeMissingValueRecords>" +
                    "<KeyWords>" +
                        "<string>Word1</string>" +
                        "<string>WORD!</string>" +
                    "</KeyWords>" +
                "</SamplesSubsetDefinitionDto>" +
                "<SamplesSubsetDefinitionDto>" +
                    "<PropertyName>Region</PropertyName>" +
                    "<AlignSubsetWithPopulation>true</AlignSubsetWithPopulation>" +
                    "<IncludeMissingValueRecords>true</IncludeMissingValueRecords>" +
                    "<KeyWords>" +
                        "<string>Utrecht</string>" +
                        "<string>GLD</string>" +
                    "</KeyWords>" +
                "</SamplesSubsetDefinitionDto>" +
            "</SamplesSubsetDefinitions>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);
            var config = settings.ConcentrationsSettings;
            var samplesDefs = config.SamplesSubsetDefinitions;
            Assert.HasCount(2, samplesDefs);
            Assert.AreEqual("Proper1", samplesDefs[0].PropertyName);
            Assert.IsTrue(samplesDefs[0].AlignSubsetWithPopulation);
            Assert.IsTrue(samplesDefs[0].IncludeMissingValueRecords);
            Assert.AreEqual("Word1 WORD!", string.Join(" ", samplesDefs[0].KeyWords));
            Assert.AreEqual("Region", samplesDefs[1].PropertyName);
            Assert.IsTrue(samplesDefs[1].AlignSubsetWithPopulation);
            Assert.IsTrue(samplesDefs[1].IncludeMissingValueRecords);
            Assert.IsTrue(samplesDefs[1].IsRegionSubset);
            Assert.AreEqual("Utrecht GLD", string.Join(" ", samplesDefs[1].KeyWords));
        }
        /// <summary>
        /// Test patch 10.00.0001
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_50_ConcentrationModelSettings() {
            var settingsXml =
            "<ConcentrationModelSettings>" +
                "<ConcentrationModelTypesPerFoodCompound>" +
                "  <ConcentrationModelTypePerFoodCompoundDto>" +
                "    <CodeFood>BANANA</CodeFood>" +
                "    <CodeCompound>Diclocrap</CodeCompound>" +
                "    <ConcentrationModelType>ZeroSpikeCensoredLogNormal</ConcentrationModelType>" +
                "  </ConcentrationModelTypePerFoodCompoundDto>" +
                "  <ConcentrationModelTypePerFoodCompoundDto>" +
                "    <CodeFood>Apple</CodeFood>" +
                "    <CodeCompound>Trifloflop</CodeCompound>" +
                "    <ConcentrationModelType>SummaryStatistics</ConcentrationModelType>" +
                "  </ConcentrationModelTypePerFoodCompoundDto>" +
                "</ConcentrationModelTypesPerFoodCompound>" +
            "</ConcentrationModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);
            var config = settings.ConcentrationModelsSettings;
            var models = config.ConcentrationModelTypesFoodSubstance;
            Assert.IsNotNull(models);
            Assert.HasCount(2, models);
            Assert.AreEqual("BANANA", models[0].FoodCode);
            Assert.AreEqual("Diclocrap", models[0].SubstanceCode);
            Assert.AreEqual(ConcentrationModelType.ZeroSpikeCensoredLogNormal, models[0].ModelType);
            Assert.AreEqual("Apple", models[1].FoodCode);
            Assert.AreEqual("Trifloflop", models[1].SubstanceCode);
            Assert.AreEqual(ConcentrationModelType.SummaryStatistics, models[1].ModelType);
        }

        /// <summary>
        /// Test patch 10.00.0001
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_50_ProjectFocalFoods() {
            var settingsXml =
            "<FocalFoods>" +
                "<FocalFoodDto>" +
                    "<CodeFood>BANANA</CodeFood>" +
                    "<CodeSubstance>30-230-33</CodeSubstance>" +
                "</FocalFoodDto>" +
                "<FocalFoodDto>" +
                    "<CodeFood>Apple</CodeFood>" +
                    "<CodeSubstance>CB123</CodeSubstance>" +
                "</FocalFoodDto>" +
                "<FocalFoodDto>" +
                    "<CodeFood>Cherry</CodeFood>" +
                    "<CodeSubstance>Fluazifazi</CodeSubstance>" +
                "</FocalFoodDto>" +
            "</FocalFoods>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);
            var focalFoods = settings.ConcentrationsSettings.FocalFoods;
            Assert.HasCount(3, focalFoods);

            Assert.AreEqual("BANANA Apple Cherry", string.Join(" ", focalFoods.Select(f => f.CodeFood)));
            Assert.AreEqual("30-230-33 CB123 Fluazifazi", string.Join(" ", focalFoods.Select(f => f.CodeSubstance)));
        }
    }
}
