using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.DeterministicSubstanceConversionFactors {
    public enum DeterministicSubstanceConversionFactorsSections {
        //No sub-sections
    }
    public class DeterministicSubstanceConversionFactorsSummarizer : ActionResultsSummarizerBase<IDeterministicSubstanceConversionFactorsActionResult> {

        public override ActionType ActionType => ActionType.DeterministicSubstanceConversionFactors;

        public override void Summarize(ProjectDto project, IDeterministicSubstanceConversionFactorsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DeterministicSubstanceConversionFactorsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new DeterministicSubstanceConversionFactorsSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.DeterministicSubstanceConversionFactors);
            subHeader.SaveSummarySection(section);
        }
    }
}
