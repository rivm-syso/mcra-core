using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class HazardCharacterisationsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.HazardCharacterisations;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            var config = project.HazardCharacterisationsSettings;
            if (config.ApplyKineticConversions) {
                config.HazardCharacterisationsConvertToSingleTargetMatrix = true;
            }
            if (config.ExposureRoutes.Count == 0
                && config.TargetDoseLevelType == TargetLevelType.External) {
                config.ExposureRoutes.Add(ExposureRoute.Oral);
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
    }
}
