using System.Globalization;
using System.Xml;
using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Utils.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class ModuleConfigBaseTests {

        [TestMethod]
        public void ModuleConfigBaseToSettingsTest() {
            var stringList = new List<string> { "Pie", "Apple", "Juice" };
            var setting = ModuleConfigBase.ToSetting(SettingsItemType.CodesSubstances, stringList);
            Assert.IsNotNull(setting?.XmlValues);
            Assert.AreEqual("Pie Apple Juice", string.Join(' ', setting.XmlValues.Select(v => v.InnerText)));

            var enumList = new List<ActionType> { ActionType.Risks, ActionType.AOPNetworks, ActionType.Concentrations };
            setting = ModuleConfigBase.ToSetting(SettingsItemType.ClusterMethodType, enumList);
            Assert.AreEqual("Risks AOPNetworks Concentrations", string.Join(' ', setting.XmlValues.Select(v => v.InnerText)));

            var boolList = new List<bool> { true, false, false, true, false };
            setting = ModuleConfigBase.ToSetting(SettingsItemType.ClusterMethodType, boolList);
            Assert.AreEqual("true false false true false", string.Join(' ', setting.XmlValues.Select(v => v.InnerText)));

            var intList = new List<int> { 4329042, 29304, -2134, -1 };
            setting = ModuleConfigBase.ToSetting(SettingsItemType.ClusterMethodType, intList);
            Assert.AreEqual("4329042 29304 -2134 -1", string.Join(' ', setting.XmlValues.Select(v => v.InnerText)));
            var reList = ModuleConfigBase.ConvertList<int>(setting);
            Assert.AreEqual("4329042 29304 -2134 -1",string.Join(' ', reList));
        }

        [TestMethod]
        public void ModuleConfigBaseConvertListTest() {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<doc><Value>  asdfsfda\r\n\r   </Value>\r\n<Value>as dsf\r    </Value><Value>\r\r\n  \nas\tdsa\n</Value></doc>");
            var setting = new ModuleSetting {
                Id = SettingsItemType.CodesSubstances,
                XmlValues =  xmlDoc.DocumentElement.ChildNodes.Cast<XmlElement>().ToArray()
            };

            var list = ModuleConfigBase.ConvertList<string>(setting);
            Assert.IsNotNull(list);
            Assert.AreEqual("asdfsfda,as dsf,as\tdsa", string.Join(',', list));

            xmlDoc.LoadXml("<doc>\r\r\n<Value>\n\t\t0.1</Value> <Value>0.2\t</Value><Value>0.3</Value>\n<Value>0.4\r\r</Value><Value>\t    0.5\r</Value></doc>");
            setting = new ModuleSetting {
                Id = SettingsItemType.SelectedPercentiles,
                XmlValues = xmlDoc.DocumentElement.ChildNodes.Cast<XmlElement>().ToArray()
            };
            var floatList = ModuleConfigBase.ConvertList<float>(setting);
            Assert.AreEqual("0.1 0.2 0.3 0.4 0.5", string.Join(' ', floatList.Select(t => t.ToString(CultureInfo.InvariantCulture))));
        }

        [TestMethod]
        public void ModuleConfig_ConcentrationModelsModuleSerializeTest() {
            var model = new ConcentrationModelsModuleConfig {
                ConcentrationModelTypesFoodSubstance = [
                    new() { FoodCode = "food", ModelType = ConcentrationModelType.MaximumResidueLimit, SubstanceCode = "subst" }
                ],
                FractionOfLor = 0.03021,
                FractionOfMrl = 21.329320,
                IsFallbackMrl = true,
                IsParametric = true,
                SelectedTier = SettingsTemplateType.Efsa2022DietaryCraAcuteTier1,
                DefaultConcentrationModel  =   ConcentrationModelType.ZeroSpikeCensoredLogNormal
            };
            var ser = model.AsConfiguration();

            Assert.IsNotNull(ser);

            var xml = XmlSerialization.ToXml(ser);

            Assert.IsNotNull(xml);
        }
    }
}
