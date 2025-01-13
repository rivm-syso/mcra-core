using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {

    public sealed class BiologicalMatrixConcentrationComparisonsSettingsSummarizer : ActionModuleSettingsSummarizer<BiologicalMatrixConcentrationComparisonsModuleConfig> {
        public BiologicalMatrixConcentrationComparisonsSettingsSummarizer(BiologicalMatrixConcentrationComparisonsModuleConfig config): base(config) {
        }

        public override ActionType ActionType => ActionType.BiologicalMatrixConcentrationComparisons;

        public override ActionSettingsSummary Summarize(ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
            section.SummarizeSetting(SettingsItemType.CorrelateTargetConcentrations, _configuration.CorrelateTargetConcentrations);
            return section;
        }
    }
}
