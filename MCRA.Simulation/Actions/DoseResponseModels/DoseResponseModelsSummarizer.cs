using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.DoseResponseModels {
    public enum DoseResponseModelsSections {
    }
    public sealed class DoseResponseModelsSummarizer : ActionResultsSummarizerBase<DoseResponseModelsActionResult> {

        public override ActionType ActionType => ActionType.DoseResponseModels;

        public override void Summarize(ProjectDto project, DoseResponseModelsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DoseResponseModelsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new DoseResponseModellingSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(
                data.SelectedResponseExperiments,
                data.DoseResponseModels,
                data.FocalEffectRepresentations,
                result?.ReferenceSubstance
            );
            int count = 1;
            foreach (var record in section.DoseResponseModels) {
                if (record.Converged) {
                    var subSubHeader = subHeader.AddSubSectionHeaderFor(record, $"{record.ResponseCode} ({record.ExperimentCode} {record.ModelType})", count++);
                    subSubHeader.SaveSummarySection(record);
                }
            }
            subHeader.SaveSummarySection(section);
        }
    }
}
