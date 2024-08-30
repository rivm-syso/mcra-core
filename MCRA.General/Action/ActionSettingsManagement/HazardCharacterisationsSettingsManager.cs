using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class HazardCharacterisationsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.HazardCharacterisations;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            var config = project.HazardCharacterisationsSettings;

            if (config.TargetDosesCalculationMethod != TargetDosesCalculationMethod.InVivoPods) {
                config.UseDoseResponseModels = true;
            }
            if (project.ActionType == ActionType.HazardCharacterisations || project.ActionType == ActionType.RelativePotencyFactors) {
                config.Aggregate = config.TargetDoseLevelType == TargetLevelType.Internal;
            }
        }

        public static List<HazardDoseImputationMethodType> AvailableHazardDoseImputationMethodTypes(bool hasHazardData) {
            var result = new List<HazardDoseImputationMethodType>() {
                    HazardDoseImputationMethodType.MunroP5,
                    HazardDoseImputationMethodType.MunroUnbiased,
                };
            if (hasHazardData) {
                result.Add(HazardDoseImputationMethodType.HazardDosesP5);
                result.Add(HazardDoseImputationMethodType.HazardDosesUnbiased);
            }
            return result;
        }

        public static bool UseKineticConversion(HazardCharacterisationsModuleConfig moduleConfig) {
            return moduleConfig.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms
                || (moduleConfig.TargetDoseLevelType == TargetLevelType.Internal && moduleConfig.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVivoPods)
                || (moduleConfig.TargetDoseLevelType == TargetLevelType.External && moduleConfig.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds);
        }
    }
}
