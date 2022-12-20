using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Substances {

    public class SubstancesSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Substances;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            if (project.AssessmentSettings.MultipleSubstances && project.AssessmentSettings.Cumulative) {
                var es = project.EffectSettings;
                section.SummarizeSetting(SettingsItemType.CodeReferenceCompound, es.CodeReferenceCompound, !string.IsNullOrEmpty(es.CodeReferenceCompound));
            }
            if (project.AssessmentSettings.MultipleSubstances) {
                section.SummarizeSetting(SettingsItemType.MultipleSubstances, project.AssessmentSettings.MultipleSubstances);
            }

            if (!project.AssessmentSettings.Cumulative) {
                section.SummarizeSetting(SettingsItemType.Cumulative, project.AssessmentSettings.Cumulative, isVisible: false);
            }
            return section;
        }
    }
}
