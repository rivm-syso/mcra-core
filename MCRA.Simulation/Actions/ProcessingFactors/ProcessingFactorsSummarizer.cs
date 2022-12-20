using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ProcessingFactors {

    public enum ProcessingFactorsSections {
        ProcessingFactorModelsSection,
        ProcessingFactorsSection
    }

    public sealed class ProcessingFactorsSummarizer : ActionResultsSummarizerBase<IProcessingFactorsActionResult> {

        public override ActionType ActionType => ActionType.ProcessingFactors;

        public override void Summarize(
            ProjectDto project,
            IProcessingFactorsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<ProcessingFactorsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            if (data.ProcessingFactorModels != null && outputSettings.ShouldSummarize(ProcessingFactorsSections.ProcessingFactorModelsSection)) {
                summarizeProcessingFactorModels(data, subHeader, subOrder++);
            }
            if (data.ProcessingFactors != null && outputSettings.ShouldSummarize(ProcessingFactorsSections.ProcessingFactorsSection)) {
                summarizeProcessingFactors(data, subHeader, subOrder++);
            }
        }

        private void summarizeProcessingFactorModels(ActionData data, SectionHeader header, int order) {
            var section = new ProcessingFactorModelSection() {
                SectionLabel = getSectionLabel(ProcessingFactorsSections.ProcessingFactorModelsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Processing factor models",
                order++
            );
            section.Summarize(data.ProcessingFactorModels.Values);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeProcessingFactors(ActionData data, SectionHeader header, int order) {
            var section = new ProcessingFactorDataSection() {
                SectionLabel = getSectionLabel(ProcessingFactorsSections.ProcessingFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Processing factors",
                order++
            );
            section.Summarize(data.ProcessingFactors);
            subHeader.SaveSummarySection(section);
        }
    }
}
