using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.OccurrencePatterns {
    public enum OccurrencePatternsSections {
        //No sub-sections
    }
    public sealed class OccurrencePatternsSummarizer : ActionResultsSummarizerBase<OccurrencePatternsActionResult> {

        public override ActionType ActionType => ActionType.OccurrencePatterns;

        public override void Summarize(ActionModuleConfig sectionConfig, OccurrencePatternsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<OccurrencePatternsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new OccurrencePatternMixtureSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.MarginalOccurrencePatterns);
            subHeader.SaveSummarySection(section);
        }
    }
}
