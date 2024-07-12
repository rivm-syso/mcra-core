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

            foreach (var actionType in Enum.GetValues<ActionType>()) {
                var tiers = templatesInstance.GetModuleTemplate(actionType);
                if (tiers?.Any() ?? false) {
                    //var definition = McraModuleDefinitions.Instance.ModuleDefinitions[actionType];
                    var moduleConfig = projectSettings.GetModuleConfiguration(actionType)?.AsConfiguration();
                    Assert.IsNotNull(moduleConfig, $"A settings configuration for module {actionType} does not exist.");
                    foreach (var tier in tiers) {
                        foreach (var setting in tier.Value.Settings) {
                            //check the moduleConfig settings which contains only own settings items
                            Assert.IsTrue(moduleConfig.SettingsDictionary.ContainsKey(setting.Id),
                                $"Tier {tier.Key} setting '{setting.Id}' is not saved in module {actionType}"
                            );
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ActionSettingsTemplates_TestGetAllTierSettings() {
            var templatesInstance = McraTemplatesCollection.Instance;
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            foreach (var definition in definitions.Values) {
                var tiers = templatesInstance.GetModuleTemplate(definition.ActionType);
                if (tiers?.Any() ?? false) {
                    foreach (var tier in tiers) {
                        var allTierSettings = definition.GetAllTierSettings(tier.Key, true);
                        foreach (var setting in tier.Value.Settings) {
                            Assert.IsTrue(allTierSettings.Contains(setting.Id));
                        }
                        foreach (var input in definition.Inputs) {
                            var inputModule = McraModuleDefinitions.Instance.ModuleDefinitions[input];
                            var inputTierSettings = inputModule.GetAllTierSettings(tier.Key, true);
                            Assert.IsTrue(inputTierSettings.All(r => allTierSettings.Contains(r)));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ActionSettingsTemplates_TestSetTierRecursive() {
            foreach (var actionType in Enum.GetValues<ActionType>()) {
                var settingsManager = ActionSettingsManagerFactory.Create(actionType);
                if (settingsManager != null) {
                    var settings = new ProjectDto { ActionType = actionType };

                    foreach (var tier in Enum.GetValues<SettingsTemplateType>()) {
                        settingsManager.SetTier(settings, tier, true);
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
            var tiers = McraTemplatesCollection.Instance.GetModuleTemplate(actionType);
            if (tiers != null && tiers.TryGetValue(tier, out var template)) {
                var tierSettings = template.Settings;
                var config = settings.GetModuleConfiguration(actionType).AsConfiguration();
                foreach (var tierSetting in tierSettings) {
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
