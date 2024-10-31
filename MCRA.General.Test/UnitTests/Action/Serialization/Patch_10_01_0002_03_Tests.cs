using System.Xml;
using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0002_03_Tests : ProjectSettingsSerializerTestsBase {

        /// <summary>
        /// Test patch replacement of Aggregate setting of HazardCharacterisations module
        /// by ExposureSources setting in TargetExposures module and test default of 
        /// new IndividualReferenceSet setting.
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="aggregate"></param>
        [TestMethod]
        [DataRow(ActionType.TargetExposures, true)]
        [DataRow(ActionType.TargetExposures, false)]
        [DataRow(ActionType.HazardCharacterisations, true)]
        [DataRow(ActionType.HazardCharacterisations, false)]
        public void Patch_10_01_0002_03_Test(
            ActionType actionType,
            bool aggregate
        ) {
            var xmlOld = createActionXml(actionType, aggregate);
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            // Assert that setting does not exist anymore in transformed XML
            var doc = new XmlDocument();
            doc.LoadXml(newXml);
            Assert.IsNotNull(doc);

            // Assert that setting does not exist anymore in transformed XML
            var xPathBase = $"//Project/ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting";
            Assert.IsNull(doc.SelectSingleNode(xPathBase + "[@id='Aggregate']"));

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);
            ExposureRoute[] expectedRoutes = aggregate
                ? [ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation]
                : [ExposureRoute.Oral];
            ExposureSource[] expectedSources = aggregate
                ? [ExposureSource.DietaryExposures, ExposureSource.OtherNonDietary]
                : [ExposureSource.DietaryExposures];
            CollectionAssert.AreEquivalent(expectedRoutes, settingsDto.TargetExposuresSettings.ExposureRoutes);
            CollectionAssert.AreEquivalent(expectedSources, settingsDto.TargetExposuresSettings.ExposureSources);
            Assert.IsTrue(settingsDto.TargetExposuresSettings.IndividualReferenceSet == ExposureSource.DietaryExposures);
        }

        private string createActionXml(
            ActionType actionType,
            bool aggregate
        ) {
            ModuleSettingsType moduleSettings = [
                ("HazardCharacterisations",
                    [
                        ("Aggregate", aggregate ? "true" : "false")
                    ]
                ),
                ("TargetExposures", [])
            ];

            var xml = createMockSettingsXml(moduleSettings, new(10, 1, 1), actionType);
            return xml;
        }
    }
}
