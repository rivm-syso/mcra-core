using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class DietaryExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.DietaryExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            var config = project.DietaryExposuresSettings;
            SetTier(project, project.ActionSettings.SelectedTier, ActionType);
            var cumulative = config.MultipleSubstances && config.Cumulative;

            var activeSubstancesConfig = project.ActiveSubstancesSettings;
            activeSubstancesConfig.FilterByAvailableHazardDose = cumulative;

            project.PopulationsSettings.IsCompute = true;
            if (cumulative) {
                project.RelativePotencyFactorsSettings.IsCompute = true;
            }

            var foodConversionsConfig = project.FoodConversionsSettings;
            if (config.ExposureType == ExposureType.Chronic && config.TotalDietStudy) {
                foodConversionsConfig.UseComposition = false;
            }

            project.OccurrencePatternsSettings.IsCompute = true;
            project.OccurrenceFrequenciesSettings.IsCompute = true;
            project.ActiveSubstancesSettings.IsCompute = true;
        }

        public override void Verify(ProjectDto project) {
        }

        public static List<SettingsTemplateType> AvailableTiers(ProjectDto project) {
            var result = new List<SettingsTemplateType>();
            var config = project.DietaryExposuresSettings;
            if (!config.TotalDietStudy) {
                result = McraTemplatesCollection.Instance.GetTiers(ActionType.DietaryExposures)
                    .Where(v => !v.Deprecated)
                    .Select(v => v.Tier)
                    .ToList();
            }
            if (project.ActionSettings.SelectedTier == SettingsTemplateType.EfsaPessimistic) {
                result.Add(SettingsTemplateType.EfsaPessimistic);
            }
            result.Add(SettingsTemplateType.Custom);
            return result;
        }

        public static List<IntakeModelType> AvailableIntakeModelTypes(ProjectDto project) {
            var result = new List<IntakeModelType> {
                IntakeModelType.BBN,
                IntakeModelType.LNN0
            };
            var config = project.DietaryExposuresSettings;
            if (config.ExposureType == ExposureType.Chronic) {
                result.Add(IntakeModelType.LNN);
                result.Add(IntakeModelType.OIM);
                result.Add(IntakeModelType.ISUF);
            }
            return result;
        }
    }
}
