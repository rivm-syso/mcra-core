using System.Xml;
using MCRA.General.Action.Serialization;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class Patch_10_02_0022_00_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        public void Patch_10_02_0022_00_PbkModels_RenameLifetimeYears() {
            var moduleSettings =
                "<ModuleConfigurations>" +
                    "<ModuleConfiguration module=\"PbkModels\">" +
                        "<Settings>" +
                            "<Setting id=\"LifetimeYears\">10</Setting>" +
                            "<Setting id=\"NumberOfDays\">33</Setting>" +
                        "</Settings>" +
                    "</ModuleConfiguration>" +
                "</ModuleConfigurations>";

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 2, 21));

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            var conf = settingsDto.GetModuleConfiguration(ActionType.TargetExposures) as TargetExposuresModuleConfig;
            Assert.IsNotNull(conf);
            Assert.AreEqual(10, conf.LifetimeEndAge);
            Assert.AreEqual(33, conf.NumberOfDays);
        }

        [TestMethod]
        public void Patch_10_02_0022_00_PbkModels_RemoveNonStationaryPeriodInYears() {
            var moduleSettings =
                "<ModuleConfigurations>" +
                    "<ModuleConfiguration module=\"PbkModels\">" +
                        "<Settings>" +
                            "<Setting id=\"LifetimeYears\">10</Setting>" +
                            "<Setting id=\"NonStationaryPeriodInYears\">5</Setting>" +
                            "<Setting id=\"NumberOfDays\">33</Setting>" +
                        "</Settings>" +
                    "</ModuleConfiguration>" +
                "</ModuleConfigurations>";

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 2, 21));

            var newXml = ProjectSettingsSerializer.GetTransformedSettingsXml(xmlOld);

            // Assert that setting does not exist anymore in transformed XML and that
            // other settings are not removed.
            var doc = new XmlDocument();
            doc.LoadXml(newXml);

            // Assert that setting does not exist anymore in transformed XML
            var xPathBase = "/Project/ModuleConfigurations/ModuleConfiguration[@module='PbkModels']/Settings/Setting";
            Assert.IsNull(doc.SelectSingleNode(xPathBase + "[@id='NonStationaryPeriodInYears']"));
            Assert.IsNotNull(doc.SelectSingleNode(xPathBase + "[@id='NumberOfDays']"));
        }
    }
}
