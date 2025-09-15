using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HbmSingleValueExposures {

    public class HbmSingleValueExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<HbmSingleValueExposuresModuleConfig> {

        public HbmSingleValueExposuresSettingsSummarizer(HbmSingleValueExposuresModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            return section;
        }
    }
}
