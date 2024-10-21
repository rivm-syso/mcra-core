using System.Xml;
using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0002_00_Tests : ProjectSettingsSerializerTestsBase {

        //Test: KineticConversionFactors: remove DeriveFromAbsorptionFactors
        [TestMethod]
        public void Patch_10_01_0002_00_KineticConversionFactorsModuleConfig_TestRemoveDeriveFromAbsorptionFactors() {
            ModuleSettingsType moduleSettings = [
                ("KineticConversionFactors", [
                    ("DeriveFromAbsorptionFactors", "true")
                ])
            ];
            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 1));
            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            // Assert that setting does not exist anymore in transformed XML and that
            var doc = new XmlDocument();
            doc.LoadXml(newXml);
            var xPathBase = "//Project/ModuleConfigurations/ModuleConfiguration[@module='KineticConversionFactors']/Settings/Setting";
            Assert.IsNull(doc.SelectSingleNode(xPathBase + "[@id='DeriveFromAbsorptionFactors']"));
        }
    }
}
