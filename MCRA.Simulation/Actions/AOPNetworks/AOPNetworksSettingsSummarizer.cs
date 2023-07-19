using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.AOPNetworks {

    public sealed class AOPNetworksSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.AOPNetworks;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var es = project.EffectSettings;
            section.SummarizeSetting(SettingsItemType.CodeAopNetwork, es.CodeAopNetwork, !string.IsNullOrEmpty(es.CodeAopNetwork));
            if (es.RestrictAopByFocalUpstreamEffect) {
                section.SummarizeSetting(SettingsItemType.RestrictAopByFocalUpstreamEffect, es.RestrictAopByFocalUpstreamEffect);
                section.SummarizeSetting(SettingsItemType.CodeFocalUpstreamEffect, es.CodeFocalUpstreamEffect, !string.IsNullOrEmpty(es.CodeFocalUpstreamEffect));
            }
            return section;
        }
    }
}
