using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.PointsOfDeparture {

    public class PointsOfDepartureSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.PointsOfDeparture;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            //section.SummarizeSetting(SettingsItemType.RestrictToCriticalEffect, project.EffectSettings.RestrictToCriticalEffect);
            return section;
        }
    }
}
