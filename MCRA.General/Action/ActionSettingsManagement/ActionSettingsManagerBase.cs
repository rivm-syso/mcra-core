using System.Globalization;
using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions;

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
                inputManager?.InitializeAction(project);
            }
        }

        public abstract void initializeSettings(ProjectDto project);

        public void SetTier(ProjectDto project, SettingsTemplateType tier, bool cascadeInputTiers) {
            if (cascadeInputTiers) {
                setTierRecursive(ActionType, project, tier, []);
            } else {
                var tierSettings = getTemplateSettings(tier, ActionType);
                if (tierSettings != null) {
                    _ = project.ApplySettings(ActionType, tierSettings);
                }
            }
        }

        private void setTierRecursive(
            ActionType actionType,
            ProjectDto project,
            SettingsTemplateType tier,
            HashSet<ActionType> visitedTypes
        ) {
            var tierSettings = getTemplateSettings(tier, actionType);
            if (tierSettings != null) {
                _ = project.ApplySettings(actionType, tierSettings);
            }

            var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[actionType];
            foreach (var input in moduleDefinition.Inputs.Where(t => !visitedTypes.Contains(t))) {
                visitedTypes.Add(input);
                setTierRecursive(input, project, tier, visitedTypes);
            }
        }

        private static List<ModuleSetting> getTemplateSettings(SettingsTemplateType tier, ActionType actionType) {
            List<ModuleSetting> settings = null;
            var useCustom = tier == SettingsTemplateType.Custom;
            if (!useCustom) {
                var templates = McraTemplatesCollection.Instance.GetModuleTemplate(actionType);
                if (templates != null && templates.TryGetValue(tier, out var template)) {
                    //Create a copy of the template settings list
                    settings = template.Settings.ToList();
                } else {
                    useCustom = true;
                }
            }
            return settings;
        }

        public Dictionary<SettingsTemplateType, string> GetAvailableTiers() {
            var template = McraTemplatesCollection.Instance.GetModuleTemplate(ActionType);
            var tiers = template?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Name);
            return tiers;
        }

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
                var currentTier = project.ActionSettings.SelectedTier;
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
