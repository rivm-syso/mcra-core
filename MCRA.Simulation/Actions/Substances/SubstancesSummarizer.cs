using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Substances {
    public enum SubstancesSections {
        //No sub-sections
    }
    public class SubstancesSummarizer : ActionResultsSummarizerBase<ISubstancesActionResult> {

        public override ActionType ActionType => ActionType.Substances;

        public override void Summarize(ProjectDto project, ISubstancesActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SubstancesSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SubstancesSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.AllCompounds, data.ReferenceCompound);
            subHeader.SaveSummarySection(section);
        }
    }
}
