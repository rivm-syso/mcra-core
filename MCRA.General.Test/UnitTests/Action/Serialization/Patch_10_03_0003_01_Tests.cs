using MCRA.General.Action.Serialization;
using MCRA.General.ModuleDefinitions.Settings;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    [TestClass]
    public class Patch_10_03_0003_01_Tests : ProjectSettingsSerializerTestsBase {

        [TestMethod]
        [DataRow("MinimumInBin", "BinLowerLimit")]
        [DataRow("MaximumInBin", "BinUpperLimit")]
        [DataRow("AverageInBin", "BinMidpoint")]
        public void Patch_10_03_0003_01_EnvironmentalBurdenOfDisease_RenameWithinBinExposureRepresentationMethod(string oldEnumValue, string newEnumValue) {
            var moduleSettings =
                "<ModuleConfigurations>" +
                    "<ModuleConfiguration module=\"EnvironmentalBurdenOfDisease\">" +
                        "<Settings>" +
                            "<Setting id=\"WithinBinExposureRepresentationMethod\">" + oldEnumValue + "</Setting>" +
                        "</Settings>" +
                    "</ModuleConfiguration>" +
                "</ModuleConfigurations>";

            var xmlOld = createMockSettingsXml(moduleSettings, new(10, 3, 2));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xmlOld, null, false, out _);

            var conf = settingsDto.GetModuleConfiguration(ActionType.EnvironmentalBurdenOfDisease) as EnvironmentalBurdenOfDiseaseModuleConfig;
            Assert.IsNotNull(conf);
            Assert.AreEqual(newEnumValue, conf.WithinBinExposureRepresentationMethod.ToString());
        }
    }
}
