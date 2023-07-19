using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {

    public sealed class BiologicalMatrixConcentrationComparisonsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.BiologicalMatrixConcentrationComparisons;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.CorrelateTargetConcentrations, project.HumanMonitoringSettings.CorrelateTargetConcentrations);
            return section;
        }
    }
}
