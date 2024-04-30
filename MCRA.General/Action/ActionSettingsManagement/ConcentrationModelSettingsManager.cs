using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConcentrationModelsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            _ = project.CalculationActionTypes.Add(ActionType.OccurrencePatterns);
        }

        public override void Verify(ProjectDto project) {
            var config = project.GetModuleConfiguration<ConcentrationModelsModuleConfig>();
            SetTier(project, config.ConcentrationModelChoice, false);
        }
    }
}
