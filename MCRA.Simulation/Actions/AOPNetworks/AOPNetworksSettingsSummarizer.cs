using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.AOPNetworks {

    public sealed class AOPNetworksSettingsSummarizer : ActionModuleSettingsSummarizer<AOPNetworksModuleConfig> {

        public AOPNetworksSettingsSummarizer(AOPNetworksModuleConfig config): base(config) {
        }

        public override ActionType ActionType => ActionType.AOPNetworks;

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.CodeAopNetwork, _configuration.CodeAopNetwork, !string.IsNullOrEmpty(_configuration.CodeAopNetwork));
            if (_configuration.RestrictAopByFocalUpstreamEffect) {
                section.SummarizeSetting(SettingsItemType.RestrictAopByFocalUpstreamEffect, _configuration.RestrictAopByFocalUpstreamEffect);
                section.SummarizeSetting(SettingsItemType.CodeFocalUpstreamEffect, _configuration.CodeFocalUpstreamEffect, !string.IsNullOrEmpty(_configuration.CodeFocalUpstreamEffect));
            }
            return section;
        }
    }
}
