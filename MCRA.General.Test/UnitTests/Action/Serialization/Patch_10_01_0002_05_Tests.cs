using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class Patch_10_01_0002_05_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        [DataRow(true, PopulationAlignmentMethod.MatchIndividualID)]
        [DataRow(false, PopulationAlignmentMethod.MatchCofactors)]
        public void Patch_10_01_0002_05_Test(
            bool matchSpecificIndividuals,
            PopulationAlignmentMethod expectedAlignmentMethod
        ) {
            ModuleSettingsType moduleSettings = [
                ("NonDietaryExposures",
                    [
                        ("MatchSpecificIndividuals", matchSpecificIndividuals ? "true": "false")
                    ]
                ),
                ("TargetExposures",
                    []
                )
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(expectedAlignmentMethod, settingsDto.TargetExposuresSettings.NonDietaryPopulationAlignmentMethod);
        }

        [TestMethod]
        [DataRow(true, PopulationAlignmentMethod.MatchCofactors)]
        [DataRow(false, PopulationAlignmentMethod.MatchCofactors)]
        [DataRow(false, PopulationAlignmentMethod.MatchIndividualID)]
        [DataRow(true, PopulationAlignmentMethod.MatchIndividualID)]
        public void Patch_10_01_0002_05_Test_AlreadyExists(
            bool matchSpecificIndividuals,
            PopulationAlignmentMethod currentValue
        ) {
            ModuleSettingsType moduleSettings = [
                ("NonDietaryExposures",
                    [
                        ("MatchSpecificIndividuals", matchSpecificIndividuals ? "true": "false")
                    ]
                ),
                ("TargetExposures",
                    [
                        ("NonDietaryPopulationAlignmentMethod", currentValue.ToString())
                    ]
                )
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            Assert.AreEqual(currentValue, settingsDto.TargetExposuresSettings.NonDietaryPopulationAlignmentMethod);
        }
    }
}
