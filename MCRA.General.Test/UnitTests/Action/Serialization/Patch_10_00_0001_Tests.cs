using System.Xml;
using MCRA.General.Action.Serialization;
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

            var patchedXml = applyPatch(xml, "Patch-10.00.0001.xslt");

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
        public void Patch_10_00_0002_TestRenameImputeHbmConcentrationsFromOtherMatrices_TestSettingExists(
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
        public void Patch_10_00_0002_TestRenameImputeHbmConcentrationsFromOtherMatrices_TestDefaultIfNew() {
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
    }
}
