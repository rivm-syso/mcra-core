using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class DietaryExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.DietaryExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            var config = project.GetModuleConfiguration<DietaryExposuresModuleConfig>();
            SetTier(project, config.DietaryIntakeCalculationTier, false);
            var cumulative = config.MultipleSubstances && config.Cumulative;

            var activeSubstancesConfig = project.GetModuleConfiguration<ActiveSubstancesModuleConfig>();
            activeSubstancesConfig.FilterByAvailableHazardDose = cumulative;

            project.AddCalculationAction(ActionType.Populations);
            if (cumulative) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }

            var foodConversionsConfig = project.GetModuleConfiguration<FoodConversionsModuleConfig>();
            if (config.ExposureType == ExposureType.Chronic && config.TotalDietStudy) {
                foodConversionsConfig.UseComposition = false;
            }

            project.AddCalculationAction(ActionType.OccurrencePatterns);
            project.AddCalculationAction(ActionType.OccurrenceFrequencies);
            project.AddCalculationAction(ActionType.ActiveSubstances);
        }

        public override void Verify(ProjectDto project) {
        }

        public static List<SettingsTemplateType> AvailableTiers(ProjectDto project) {
            var result = new List<SettingsTemplateType>();
            var config = project.GetModuleConfiguration<DietaryExposuresModuleConfig>();
            if (!config.TotalDietStudy) {
                result = McraTemplatesCollection.Instance.GetModuleTemplate(ActionType.DietaryExposures)
                    .Values.Where(v => !v.Deprecated)
                    .Select(v => v.Tier).ToList();
            }
            if (config.DietaryIntakeCalculationTier == SettingsTemplateType.EfsaPessimistic) {
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
            var config = project.GetModuleConfiguration<DietaryExposuresModuleConfig>();
            if (config.ExposureType == ExposureType.Chronic) {
                result.Add(IntakeModelType.LNN);
                result.Add(IntakeModelType.OIM);
                result.Add(IntakeModelType.ISUF);
            }
            return result;
        }
    }
}
