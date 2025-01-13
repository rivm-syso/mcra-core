using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PointsOfDeparture {

    public class PointsOfDepartureSettingsSummarizer : ActionModuleSettingsSummarizer<PointsOfDepartureModuleConfig> {

        public PointsOfDepartureSettingsSummarizer(PointsOfDepartureModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            //section.SummarizeSetting(SettingsItemType.RestrictToCriticalEffect, _configuration.RestrictToCriticalEffect);
            return section;
        }
    }
}
