using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.DoseResponseData {
    public enum DoseResponseDataSections {
        AvailableDoseResponseDataSection
    }
    public sealed class DoseResponseDataSummarizer : ActionResultsSummarizerBase<IDoseResponseDataActionResult> {

        public override ActionType ActionType => ActionType.DoseResponseData;

        public override void Summarize(ProjectDto project, IDoseResponseDataActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DoseResponseDataSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new DoseResponseDataSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(
                data.SelectedResponseExperiments,
                data.Responses.Values,
                subHeader,
                order++
            );
            if (data.SelectedResponseExperiments != data.AvailableDoseResponseExperiments
                && outputSettings.ShouldSummarize(DoseResponseDataSections.AvailableDoseResponseDataSection)) {
                summarizeAvailableData(
                    data.AvailableDoseResponseExperiments,
                    data.Responses,
                    subHeader,
                    order++
                );
            }
            subHeader.SaveSummarySection(section);
        }

        private void summarizeAvailableData(
                ICollection<DoseResponseExperiment> availableDoseResponseExperiments,
                IDictionary<string, Response> responses,
                SectionHeader header,
                int order
            ) {
            var section = new DoseResponseDataSection() {
                SectionLabel = getSectionLabel(DoseResponseDataSections.AvailableDoseResponseDataSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Available dose response data", order);
            section.Summarize(
                availableDoseResponseExperiments,
                responses.Values,
                subHeader,
                order++
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
