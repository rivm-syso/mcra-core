using System.Xml;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                settingsDto.HumanMonitoringSettings.TargetMatrix
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
                BiologicalMatrix.WholeBody,
                settingsDto.HumanMonitoringSettings.TargetMatrix
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
                settingsDto.HumanMonitoringSettings.TargetMatrix
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
                settingsDto.HumanMonitoringSettings.TargetMatrix
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
                settingsDto.HumanMonitoringSettings.TargetMatrix
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
                settingsDto.HumanMonitoringSettings.TargetMatrix
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
            Assert.IsTrue(settingsDto.HumanMonitoringSettings.SamplingMethodCodes.Contains("BloodSerum_Serum"));
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
            Assert.AreEqual("BANANA,PINEAPPLE", string.Join(",", settingsDto.FoodAsEatenSubset));
            Assert.AreEqual("Milk duds,Apple pie,PIZZA", string.Join(",", settingsDto.ModelledFoodSubset));
            Assert.AreEqual("Orange,Tomato,Minneola", string.Join(",", settingsDto.SelectedScenarioAnalysisFoods));
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
            Assert.AreEqual("BANANA,PINEAPPLE,Potatoes,Mushrooms", string.Join(",", settingsDto.IntakeModelSettings.IntakeModelsPerCategory.SelectMany(r => r.FoodsAsMeasured)));
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
            Assert.AreEqual(value, settingsDto.EffectSettings.HazardCharacterisationsConvertToSingleTargetMatrix);
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
            Assert.AreEqual(20, settingsDto.EffectSettings.AdditionalAssessmentFactor);
            Assert.AreEqual(true, settingsDto.EffectSettings.HazardCharacterisationsConvertToSingleTargetMatrix);
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

            Assert.AreEqual(SettingsTemplateType.Efsa2022DietaryCraChronicTier1, settings.RisksSettings.RiskCalculationTier);
            Assert.AreEqual(SettingsTemplateType.Efsa2022DietaryCraAcuteTier2, settings.RisksSettings.SingleValueRisksCalculationTier);
            Assert.AreEqual(HealthEffectType.Benefit, settings.RisksSettings.HealthEffectType);
            Assert.AreEqual(1.23456, settings.RisksSettings.LeftMargin);
            Assert.AreEqual(2.34567, settings.RisksSettings.RightMargin);
            Assert.IsTrue(settings.RisksSettings.IsEAD);
            Assert.AreEqual(3.45678, settings.RisksSettings.ThresholdMarginOfExposure);
            Assert.AreEqual(4.56789, settings.RisksSettings.ConfidenceInterval);
            Assert.AreEqual(5.67891, settings.RisksSettings.DefaultInterSpeciesFactorGeometricMean);
            Assert.AreEqual(6.78912, settings.RisksSettings.DefaultInterSpeciesFactorGeometricStandardDeviation);
            Assert.AreEqual(7.89123, settings.RisksSettings.DefaultIntraSpeciesFactor);
            Assert.AreEqual(888, settings.RisksSettings.NumberOfLabels);
            Assert.IsTrue(settings.RisksSettings.CumulativeRisk);
            Assert.AreEqual(RiskMetricType.ExposureHazardRatio, settings.RisksSettings.RiskMetricType);
            Assert.AreEqual(RiskMetricCalculationType.SumRatios, settings.RisksSettings.RiskMetricCalculationType);
            Assert.AreEqual(123, settings.RisksSettings.NumberOfSubstances);
            Assert.AreEqual(SingleValueRiskCalculationMethod.FromIndividualRisks, settings.RisksSettings.SingleValueRiskCalculationMethod);
            Assert.AreEqual(67.89123, settings.RisksSettings.Percentage);
            Assert.IsTrue(settings.RisksSettings.IsInverseDistribution);
            Assert.IsTrue(settings.RisksSettings.UseAdjustmentFactors);
            Assert.AreEqual(AdjustmentFactorDistributionMethod.Gamma, settings.RisksSettings.ExposureAdjustmentFactorDistributionMethod);
            Assert.AreEqual(AdjustmentFactorDistributionMethod.LogNormal, settings.RisksSettings.HazardAdjustmentFactorDistributionMethod);
            Assert.AreEqual(9, settings.RisksSettings.ExposureParameterA);
            Assert.AreEqual(10, settings.RisksSettings.ExposureParameterB);
            Assert.AreEqual(11, settings.RisksSettings.ExposureParameterC);
            Assert.AreEqual(12, settings.RisksSettings.ExposureParameterD);
            Assert.AreEqual(13, settings.RisksSettings.HazardParameterA);
            Assert.AreEqual(14, settings.RisksSettings.HazardParameterB);
            Assert.AreEqual(15, settings.RisksSettings.HazardParameterC);
            Assert.AreEqual(16, settings.RisksSettings.HazardParameterD);
            Assert.IsTrue(settings.RisksSettings.UseBackgroundAdjustmentFactor);
            Assert.IsTrue(settings.RisksSettings.CalculateRisksByFood);
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
            Assert.AreEqual(IntakeModelType.BBN, settings.IntakeModelSettings.IntakeModelType);
            Assert.AreEqual(TransformType.Power, settings.IntakeModelSettings.TransformType);
            Assert.AreEqual(456, settings.IntakeModelSettings.GridPrecision);
            Assert.AreEqual(789, settings.IntakeModelSettings.NumberOfIterations);
            Assert.IsTrue(settings.IntakeModelSettings.SplineFit);
            Assert.IsTrue(settings.IntakeModelSettings.CovariateModelling);
            Assert.IsTrue(settings.IntakeModelSettings.FirstModelThenAdd);
            Assert.AreEqual(0.12345, settings.IntakeModelSettings.Dispersion);
            Assert.AreEqual(1.23456, settings.IntakeModelSettings.VarianceRatio);
            var models = settings.IntakeModelSettings.IntakeModelsPerCategory;
            Assert.IsNotNull(models);
            Assert.AreEqual(2, models.Count);
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

            var individualsDefs = settings.IndividualsSubsetDefinitions;
            Assert.AreEqual(2, individualsDefs.Count);
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

            var samplesDefs = settings.SamplesSubsetDefinitions;
            Assert.AreEqual(2, samplesDefs.Count);
            Assert.AreEqual("Proper1", samplesDefs[0].PropertyName);
            Assert.IsTrue(samplesDefs[0].AlignSubsetWithPopulation);
            Assert.IsTrue(samplesDefs[0].IncludeMissingValueRecords);
            Assert.AreEqual("Word1 WORD!", string.Join(" ", samplesDefs[0].KeyWords));
            Assert.AreEqual("Region", samplesDefs[1].PropertyName);
            Assert.IsTrue(samplesDefs[1].AlignSubsetWithPopulation);
            Assert.IsTrue(samplesDefs[1].IncludeMissingValueRecords);
            Assert.IsTrue(samplesDefs[1].IsRegionSubset());
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
            var models = settings.ConcentrationModelSettings.ConcentrationModelTypesFoodSubstance;
            Assert.IsNotNull(models);
            Assert.AreEqual(2, models.Count);
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
            Assert.AreEqual(3, settings.FocalFoods.Count);

            Assert.AreEqual("BANANA Apple Cherry", string.Join(" ", settings.FocalFoods.Select(f => f.CodeFood)));
            Assert.AreEqual("30-230-33 CB123 Fluazifazi", string.Join(" ", settings.FocalFoods.Select(f => f.CodeSubstance)));
        }
    }
}
