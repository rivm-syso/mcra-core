using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.SettingTemplates {
    [TestClass]
    public class ActionSettingsTemplatesTests {
        [TestMethod]
        public void ActionSettingsTemplates_TestTiers() {
            var templatesInstance = McraTemplatesCollection.Instance;
            var projectSettings = new ProjectDto();

            var templateTypes = Enum.GetValues<SettingsTemplateType>()
                .Where(t => t != SettingsTemplateType.Custom);

            foreach (var templateType in templateTypes) {
                var template = templatesInstance.GetTemplate(templateType);
                Assert.IsNotNull(template, $"A settings template for tier {templateType} does not exist.");
                foreach(var config in template.ModuleConfigurations) {
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
                        Assert.AreEqual(0, errors.Count, string.Join('\n', errors));
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

            if(template.ConfigurationsDictionary.TryGetValue(actionType, out var config)) {
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
