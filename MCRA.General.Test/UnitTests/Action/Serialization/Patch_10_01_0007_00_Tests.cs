using MCRA.General.Action.Serialization;
using MCRA.General.ModuleDefinitions.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class Patch_10_01_0007_00_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        public void Patch_10_01_0007_00_ExposureSources_RenameExposureSourceValues() {
            // NOTE: using old style of settings initialization because a range of Values for a setting
            //       seemed not yet to be included with the new ModuleSettingsType approach.
            var moduleSettings =
                "<ModuleConfigurations>" +
                    "<ModuleConfiguration module=\"TargetExposures\" compute=\"false\">" +
                        "<Settings>" +
                            "<Setting id=\"ExposureSources\">" +
                                "<Value>DietaryExposures</Value>" +
                                "<Value>DustExposures</Value>" +
                                "<Value>SoilExposures</Value>" +
                                "<Value>OtherNonDietary</Value>" +
                            "</Setting>" +
                            "<Setting id=\"IndividualReferenceSet\">DietaryExposures</Setting>" +
                        "</Settings>" +
                    "</ModuleConfiguration>" +
                "</ModuleConfigurations>";

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 6));

            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            var conf = settingsDto.GetModuleConfiguration(ActionType.TargetExposures) as TargetExposuresModuleConfig;
            Assert.IsNotNull(conf);
            var values = Enum.GetValues<ExposureSource>();
            foreach (var exposureSource in conf.ExposureSources) {
                Assert.IsTrue(values.Contains(exposureSource));
            }
        }

        [TestMethod]
        [DataRow("DietaryExposures")]
        [DataRow("DustExposures")]
        [DataRow("SoilExposures")]
        [DataRow("OtherNonDietary")]
        public void Patch_10_01_0007_00_IndividualReferenceSet_RenameExposureSourceValues(string oldEnumValue) {
            ModuleSettingsType moduleSettings = [
                ("TargetExposures",
                    [
                        ("IndividualReferenceSet", oldEnumValue )
                    ]
                ),
            ];

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 1, 6));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            var conf = settingsDto.GetModuleConfiguration(ActionType.TargetExposures) as TargetExposuresModuleConfig;
            Assert.IsNotNull(conf);
            var values = Enum.GetValues<ExposureSource>();
            Assert.IsTrue(values.Contains(conf.IndividualReferenceSet));
        }
    }
}
