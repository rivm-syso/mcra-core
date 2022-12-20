using MCRA.Utils.ExtensionMethods;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.ModuleDefinitions;
using MCRA.General.SettingsDefinitions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MCRA.General.Action.ActionSettingsManagement {
    public abstract class ActionSettingsManagerBase : IActionSettingsManager {

        public abstract ActionType ActionType { get; }

        public abstract void Verify(ProjectDto project);

        public void InitializeAction(ProjectDto project) {
            initializeSettings(project);
            var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[ActionType];
            foreach (var input in moduleDefinition.Inputs) {
                var inputModule = McraModuleDefinitions.Instance.ModuleDefinitions[input];
                var inputManager = ActionSettingsManagerFactory.Create(inputModule.ActionType);
                if (inputManager != null) {
                    inputManager.InitializeAction(project);
                }
            }
        }

        public abstract void initializeSettings(ProjectDto project);

        public void SetTier(ProjectDto project, string idTier, bool cascadeInputTiers) {
            setTierSelectionEnumSetting(project, idTier);
            var tier = getTier(project, idTier);
            if (tier != null) {
                SetTier(project, tier, cascadeInputTiers);
            }
        }

        public void SetTier(ProjectDto project, ModuleTier tier, bool cascadeInputTiers) {
            foreach (var setting in tier.TierSettings) {
                setSetting(project, setting.IdSetting, setting.Value);
            }
            if (cascadeInputTiers) {
                foreach (var inputTier in tier.InputTiers) {
                    var inputModule = McraModuleDefinitions.Instance.ModuleDefinitionsById[inputTier.Input];
                    var inputManager = ActionSettingsManagerFactory.Create(inputModule.ActionType);
                    if (inputManager != null) {
                        inputManager.SetTier(project, inputTier.Tier, cascadeInputTiers);
                    }
                }
            }
        }

        private ModuleTier getTier(ProjectDto project, string idTier) {
            var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[ActionType];
            if (!string.IsNullOrEmpty(idTier) && !idTier.Equals("custom", StringComparison.OrdinalIgnoreCase)) {
                var tier = moduleDefinition.Tiers?.FirstOrDefault(r => r.Id.Equals(idTier, StringComparison.OrdinalIgnoreCase));
                if (tier == null) {
                    throw new Exception($"Unknown tier {idTier} for action {ActionType}.");
                }
                return tier;
            }
            return null;
        }

        public Dictionary<string, string> GetAvailableTiers() {
            var enumName = getTierSelectionEnumName();
            if (!string.IsNullOrEmpty(enumName)) {
                var result = new Dictionary<string, string>();
                var assembly = Assembly.GetAssembly(typeof(SettingsItem));
                var enumType = assembly.GetType("MCRA.General." + enumName, true, true);
                var values = enumType.GetEnumValues();
                foreach (Enum value in values) {
                    result.Add(value.ToString(), value.GetDisplayName());
                }
                return result;
            }
            return null;
        }

        protected abstract string getTierSelectionEnumName();

        protected abstract void setTierSelectionEnumSetting(ProjectDto project, string idTier);

        protected abstract void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue);

        protected bool parseBoolSetting(string rawValue) {
            return bool.Parse(rawValue);
        }

        protected double parseDoubleSetting(string rawValue) {
            return double.Parse(rawValue, CultureInfo.InvariantCulture);
        }

        protected int parseIntSetting(string rawValue) {
            return int.Parse(rawValue, CultureInfo.InvariantCulture);
        }
    }
}
