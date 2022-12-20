using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.IntraSpeciesFactors {

    public sealed class IntraSpeciesFactorsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.IntraSpeciesFactors;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var ems = project.EffectModelSettings;
            section.SummarizeSetting(SettingsItemType.DefaultIntraSpeciesFactor, ems.DefaultIntraSpeciesFactor);
            return section;
        }
    }
}
