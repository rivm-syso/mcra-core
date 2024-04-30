using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public class SingleValueRisksSettingsManager : ActionSettingsManagerBase {
        public override ActionType ActionType => ActionType.SingleValueRisks;

        public override void initializeSettings(ProjectDto project) {
            //set default for new actions
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            var config = project.GetModuleConfiguration<SingleValueRisksModuleConfig>();
            SetTier(project, config.SingleValueRisksCalculationTier, false);
        }
    }
}
