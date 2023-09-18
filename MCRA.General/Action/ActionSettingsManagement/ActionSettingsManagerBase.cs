using System.Globalization;
using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions;
using MCRA.General.SettingsDefinitions;

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

        public void SetTier(ProjectDto project, SettingsTemplateType tier, bool cascadeInputTiers) {
            var settings = getTemplateSettings(tier);
            if (settings != null) {
                foreach (var setting in settings) {
                    setSetting(project, setting.Id, setting.Value);
                }
            }
            if (cascadeInputTiers) {
                var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[ActionType];
                foreach (var input in moduleDefinition.Inputs) {
                    var inputManager = ActionSettingsManagerFactory.Create(input);
                    inputManager?.SetTier(project, tier, cascadeInputTiers);
                }
            }
        }

        public virtual SettingsTemplateType GetTier(ProjectDto project) => SettingsTemplateType.Custom;

        private List<ModuleSetting> getTemplateSettings(SettingsTemplateType tier) {
            List<ModuleSetting> settings = null;
            var useCustom = tier == SettingsTemplateType.Custom;
            if (!useCustom) {
                var templates = McraTemplatesCollection.Instance.GetModuleTemplate(ActionType);
                if (templates != null && templates.TryGetValue(tier, out var template)) {
                    //Create a copy of the template settings list
                    settings = template.Settings.ToList();
                } else {
                    useCustom = true;
                }
            }

            //If the module definition has a tier selection setting,
            //it's added dynamically to the settings list here
            var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[ActionType];
            var tierSettingsItem = moduleDefinition.TierSelectionSetting;
            if (!string.IsNullOrEmpty(tierSettingsItem) &&
                Enum.TryParse<SettingsItemType>(tierSettingsItem, out var tierSettingType)
            ) {
                if (useCustom) {
                    //if this module doesn't explicitly define the requested tier,
                    //create a new settings list with only one the tier setting set to Custom
                    var tierSetting = new ModuleSetting { Id = tierSettingType, ReadOnly = true, Value = SettingsTemplateType.Custom.ToString() };
                    settings = new List<ModuleSetting> { tierSetting };
                } else {
                    //add the tier setting attribute to the settings list here
                    var tierSetting = new ModuleSetting { Id = tierSettingType, ReadOnly = true, Value = tier.ToString() };
                    settings.Add(tierSetting);
                }
            }
            return settings;
        }

        public Dictionary<SettingsTemplateType, string> GetAvailableTiers() {
            var template = McraTemplatesCollection.Instance.GetModuleTemplate(ActionType);
            var tiers = template?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Name);
            return tiers;
        }

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

        public static void ChangeActionType(ProjectDto project, ActionType newActionType) {
            if (project.ActionType == newActionType) {
                return;
            }

            var currentManager = ActionSettingsManagerFactory.Create(project.ActionType);
            var newManager = ActionSettingsManagerFactory.Create(newActionType);
            //the new tier will be custom unless new action type contains the same tier
            var newTier = SettingsTemplateType.Custom;

            //check whether we have compatible tiers
            if(currentManager != null && newManager != null) {
                var currentTier = currentManager.GetTier(project);
                var newTiers = newManager.GetAvailableTiers();
                if (newTiers != null && newTiers.ContainsKey(currentTier)) {
                    //only if tiers are compatible, set it to the current tier
                    newTier = currentTier;
                }
            }

            project.ActionType = newActionType;

            if(newManager != null) {
                newManager.SetTier(project, newTier, true);
            } else {
                //set all tiers recursively for any submodules of this module
                var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[newActionType];
                foreach (var input in moduleDefinition.Inputs) {
                    var inputManager = ActionSettingsManagerFactory.Create(input);
                    inputManager?.SetTier(project, newTier, true);
                }
            }
        }
    }
}
