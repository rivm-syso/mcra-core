﻿using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.SettingTemplates {
    [TestClass]
    public class ActionSettingsTemplatesTests {
        [TestMethod]
        public void ActionSettingsTemplates_TestTiers() {
            var templatesInstance = McraTemplatesCollection.Instance;
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            foreach (var definition in definitions.Values) {
                var tiers = templatesInstance.GetModuleTemplate(definition.ActionType);
                if (tiers?.Any() ?? false) {
                    foreach (var tier in tiers) {
                        foreach (var setting in tier.Value.Settings) {
                            Assert.IsTrue(definition.AllModuleSettings.Contains(setting.Id));
                        }
                        //check hierarchical availability of the tier in input modules
                        //which have templated settings
                        foreach (var input in definition.Inputs) {
                            var inputModule = McraModuleDefinitions.Instance.ModuleDefinitions[input];
                            var hasSettings = inputModule.TemplateSettings?.Any() ?? false;
                            Assert.AreEqual(hasSettings, !string.IsNullOrEmpty(inputModule.TierSelectionSetting),
                                $"Tier {tier.Key} module {input} has {(hasSettings ? "" : "no ")} settings but {(hasSettings ? "no " : "does have a ")} TierSelectionSetting"
                            );
                            if (hasSettings && inputModule.TemplateSettings.TryGetValue(tier.Key, out var settings)) {
                                var tierSelection = settings.Settings.Where(t => t.Id.ToString() == inputModule.TierSelectionSetting).FirstOrDefault();
                                Assert.IsNull(tierSelection, $"Tier {tier.Key} module {input} should not define '{inputModule.TierSelectionSetting}', it is implied.");
                            }
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
    }
}