using System.Xml;
using System.Xml.Linq;
using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0001_02_Tests : ProjectSettingsSerializerTestsBase {

        //Test: KineticConversionFactors KineticModels and no TargetExposures
        // KineticConversionFactors module contains the following settings that are migrated from the KineticModels module:
        // - KCFSubgroupDependent
        // - ResampleKineticConversionFactors (from ResampleKineticModelParameters KineticModels module)
        [TestMethod]
        public void Patch_10_01_0001_02_Tests_TestRemoveCodeKineticModel() {
            ModuleSettingsType moduleSettings = [
                ("KineticModels", [
                    ("CodeKineticModel", "EuroMix_Generic_PBTK_model_V6"),
                    ("KCFSubgroupDependent", "true"),
                    ("ResampleKineticModelParameters","true"),
                ])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 0));

            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            // Assert that setting does not exist anymore in transformed XML and that
            // other settings are not removed.
            var doc = new XmlDocument();
            doc.LoadXml(newXml);
            var xPathBase = "//Project/ModuleConfigurations/ModuleConfiguration[@module='PbkModels']/Settings/Setting";
            Assert.IsNull(doc.SelectSingleNode(xPathBase + "[@id='CodeKineticModel']"));
            Assert.AreEqual(
                "true",
                doc.SelectSingleNode(xPathBase + "[@id='ResamplePbkModelParameters']").InnerText
            );
        }
    }
}
