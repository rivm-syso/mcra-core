using System.Collections.ObjectModel;
using MCRA.Utils.Xml;
using System.Xml;
using System.Xml.Serialization;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Test.Helpers;

namespace MCRA.General.Test.UnitTests.SettingTemplates {

    [TestClass]
    public class ActionSettingsTemplatesTests {

        #region Private classes
        /// <summary>
        /// Test classes to deserialize templates to test
        /// consistency of template settings with module definitions
        /// </summary>
        [Serializable]
        [XmlType("SettingsTemplates")]
        public sealed class TestSettingsTemplates : Collection<TestSettingsTemplate> {
        }

        public sealed class TestModuleConfiguration {
            [XmlAttribute("module")]
            public string ModuleName { get; set; }
            [XmlArrayItem("Setting")]
            public TestModuleSetting[] Settings { get; set; }
            [XmlArrayItem("Setting")]
            public TestModuleSetting[] ExcludedSettings { get; set; }
            public override string ToString() => ModuleName;
        }

        [XmlType("SettingsTemplate")]
        public class TestSettingsTemplate {
            public string Id { get; set; }
            [XmlArrayItem("ModuleConfiguration")]
            public TestModuleConfiguration[] ModuleConfigurations { get; set; }
        }

        public class TestModuleSetting {
            [XmlAttribute("id")]
            public string Id { get; set; }
            [XmlText]
            public string Value { get; set; }
        }
        #endregion

        [TestMethod]
        public void ActionSettingsTemplates_TestSettingsConsistency() {
            var templatesFileName = Path.Combine("UnitTests", "ActionSettingsTemplates", "SettingsTemplates.Generated.xml");
            var templatesXml = File.ReadAllText(templatesFileName);
            var templates = XmlSerialization.FromXml<TestSettingsTemplates>(templatesXml);
            var settings = new ProjectDto();
            var outputFolder = Path.Combine(TestUtilities.TestOutputPath, "ActionTiersFixes");
            Directory.CreateDirectory(outputFolder);
            //collect invalid settings in a list
            var errors = new List<string>();

            foreach (var template in templates) {
                var outputTemplateFileName = Path.Combine(outputFolder, $"{template.Id}.xml");
                using(var outputTemplateFileWriter = new StreamWriter(outputTemplateFileName)) {
                    outputTemplateFileWriter.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    outputTemplateFileWriter.WriteLine("<SettingsTemplate>");
                    outputTemplateFileWriter.WriteLine("  <ModuleConfigurations>");
                    foreach (var templateConfig in template.ModuleConfigurations) {
                        var configType = templateConfig.ModuleName;
                        if (!Enum.TryParse<ActionType>(configType, out var actionType)) {
                            errors.Add($"Module of type '{configType}' configured in template '{template.Id}' does not exist.");
                            continue;
                        }
                        outputTemplateFileWriter.WriteLine($"    <ModuleConfiguration module=\"{templateConfig.ModuleName}\">");
                        outputTemplateFileWriter.WriteLine("      <Settings>");

                        var moduleConfig = settings.GetModuleConfiguration(actionType).AsConfiguration();
                        //check template config's settings against module config's settings
                        foreach (var setting in templateConfig.Settings) {
                            if (!Enum.TryParse<SettingsItemType>(setting.Id, out var templateSettingType)
                                || !moduleConfig.SettingsDictionary.ContainsKey(templateSettingType)
                            ) {
                                errors.Add($"Setting '{setting.Id}' is not part of module '{configType}' in template '{template.Id}').");
                            } else {
                                outputTemplateFileWriter.WriteLine($"        <Setting id=\"{setting.Id}\">{setting.Value}</Setting>");
                            }
                        }
                        outputTemplateFileWriter.WriteLine("      </Settings>");
                        outputTemplateFileWriter.WriteLine("      <ExcludedSettings>");

                        //the other way around, check whether all settings in module configuration definition are
                        //accounted for in either the settings or explicitly ignored
                        var templateSettingIds = templateConfig.Settings.Select(s => s.Id)
                            .ToHashSet();
                        var excludedSettingIds = templateConfig.ExcludedSettings?.Select(s => s.Id)
                            .ToHashSet() ?? [];
                        //write the excluded settings to the output file, do check whether they exist in module settings
                        foreach ( var excludedSettingId in excludedSettingIds) {
                            if (!Enum.TryParse<SettingsItemType>(excludedSettingId, out var templateSettingType)
                                || !moduleConfig.SettingsDictionary.ContainsKey(templateSettingType)
                            ) {
                                errors.Add($"Excluded setting '{excludedSettingId}' is not part of module '{configType}' in template '{template.Id}').");
                            } else if (templateSettingIds.Contains(excludedSettingId)) {
                                errors.Add($"Duplicate: Excluded setting '{excludedSettingId}' is already defined in template '{template.Id}').");
                            } else {
                                outputTemplateFileWriter.WriteLine($"        <Setting id=\"{excludedSettingId}\" />");
                            }
                        }
                        var moduleSettingIds = moduleConfig.SettingsDictionary.Keys
                            .Select(k => k.ToString())
                            .Order(StringComparer.OrdinalIgnoreCase);

                        foreach (var moduleSettingId in moduleSettingIds) {
                            if (!templateSettingIds.Contains(moduleSettingId) && !excludedSettingIds.Contains(moduleSettingId)) {
                                errors.Add($"Setting '{moduleSettingId}' of module '{configType}' is not defined or explicitly excluded in template '{template.Id}').");
                                //write the not included setting to the
                                outputTemplateFileWriter.WriteLine($"        <Setting id=\"{moduleSettingId}\" />");
                            }
                        }
                        outputTemplateFileWriter.WriteLine("      </ExcludedSettings>");
                        outputTemplateFileWriter.WriteLine($"    </ModuleConfiguration>");
                    }
                    outputTemplateFileWriter.WriteLine("  </ModuleConfigurations>");
                    outputTemplateFileWriter.WriteLine("</SettingsTemplate>");
                }
            }
            if (errors.Count > 0) {
                Assert.Fail($"Found invalid template settings:\n{string.Join('\n', errors)}");
            }
        }

        [TestMethod]
        public void ActionSettingsTemplates_TestTiers() {
            var templatesInstance = McraTemplatesCollection.Instance;
            var projectSettings = new ProjectDto();

            var templateTypes = Enum.GetValues<SettingsTemplateType>()
                .Where(t => t != SettingsTemplateType.Custom);

            foreach (var templateType in templateTypes) {
                var template = templatesInstance.GetTemplate(templateType);
                Assert.IsNotNull(template, $"A settings template for tier {templateType} does not exist.");
                foreach (var config in template.ModuleConfigurations) {
                    var moduleConfig = projectSettings.GetModuleConfiguration(config.ActionType)?.AsConfiguration();
                    foreach (var setting in config.Settings) {
                        //check the moduleConfig settings which contains only own settings items
                        Assert.IsTrue(moduleConfig.SettingsDictionary.ContainsKey(setting.Id),
                            $"Tier {templateType} setting '{setting.Id}' is not saved in module {config.ActionType}"
                        );
                    }
                }
            }
        }

        [TestMethod]
        public void ActionSettingsTemplates_TestSetTierRecursive() {
            var templateTypes = Enum.GetValues<SettingsTemplateType>()
                .Where(t => t != SettingsTemplateType.Custom);
            foreach (var actionType in Enum.GetValues<ActionType>()) {
                var settingsManager = ActionSettingsManagerFactory.Create(actionType);
                if (settingsManager != null) {
                    var settings = new ProjectDto { ActionType = actionType };

                    foreach (var tier in templateTypes) {
                        ActionSettingsManagerBase.SetTier(settings, tier);
                        //check current actiontype's tier settings in project config
                        var errors = checkTierSettingValuesRecursive(actionType, tier, settings, [actionType]);
                        Assert.IsEmpty(errors, string.Join('\n', errors));
                    }
                }
            }
        }

        private List<string> checkTierSettingValuesRecursive(
            ActionType actionType,
            SettingsTemplateType tier,
            ProjectDto settings,
            HashSet<ActionType> checkedTypes
        ) {
            var result = new List<string>();
            var template = McraTemplatesCollection.Instance.GetTemplate(tier);

            if (template.ConfigurationsDictionary.TryGetValue(actionType, out var config)) {
                foreach (var tierSetting in config.Settings) {
                    var projectValue = config.SettingsDictionary[tierSetting.Id].Value;
                    if (projectValue != tierSetting.Value) {
                        result.Add($"{actionType}.{tierSetting.Id}: {projectValue} != {tierSetting.Value}");
                    }
                }
            }
            var definition = McraModuleDefinitions.Instance.ModuleDefinitions[actionType];
            foreach (var inputActionType in definition.Inputs.Where(r => !checkedTypes.Contains(r))) {
                checkedTypes.Add(inputActionType);
                result.AddRange(checkTierSettingValuesRecursive(inputActionType, tier, settings, checkedTypes));
            }
            return result;
        }
    }
}
