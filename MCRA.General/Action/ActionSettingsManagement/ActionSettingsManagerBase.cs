using System.Globalization;
using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions;
using MCRA.Utils.ExtensionMethods;

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

        public static void SetTier(ProjectDto project, SettingsTemplateType tier) {
            project.ActionSettings.SelectedTier = tier;
            if (tier != SettingsTemplateType.Custom) {
                var template = McraTemplatesCollection.Instance.GetTemplate(tier);
                project.ApplySettings(template.ModuleConfigurations);
            }
        }

        public static void SetTier(ProjectDto project, SettingsTemplateType tier, ActionType actionType) {
            if(tier != SettingsTemplateType.Custom) {
                var template = McraTemplatesCollection.Instance.GetTemplate(tier);
                if (template.ConfigurationsDictionary.TryGetValue(actionType, out var actionConfig)) {
                    project.ApplySettings(actionConfig);
                }
            }
        }

        public Dictionary<SettingsTemplateType, string> GetAvailableTiers() {
            var templates = McraTemplatesCollection.Instance.GetTiers(ActionType);
            var tiers = templates?.ToDictionary(t => t.Tier, t => t.Name);
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
            if (currentManager != null && newManager != null) {
                var currentTier = project.ActionSettings.SelectedTier;
                var newTiers = newManager.GetAvailableTiers();
                if (newTiers != null && newTiers.ContainsKey(currentTier)) {
                    //only if tiers are compatible, set it to the current tier
                    newTier = currentTier;
                }
            }

            project.ActionType = newActionType;

            SetTier(project, newTier);
        }
    }
}
