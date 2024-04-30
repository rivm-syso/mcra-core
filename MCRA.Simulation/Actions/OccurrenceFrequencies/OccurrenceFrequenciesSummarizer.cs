using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.OccurrenceFrequencies {
    public enum OccurrenceFrequenciesSections {
        //No sub-sections
    }
    public sealed class OccurrenceFrequenciesSummarizer : ActionResultsSummarizerBase<OccurrenceFrequenciesActionResult> {

        public override ActionType ActionType => ActionType.OccurrenceFrequencies;

        public override void Summarize(ActionModuleConfig sectionConfig, OccurrenceFrequenciesActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<OccurrenceFrequenciesSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new OccurrenceFrequenciesSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.OccurrenceFractions, data.SubstanceAuthorisations);
            subHeader.SaveSummarySection(section);
        }

        public void SummarizeUncertain(ActionData data, SectionHeader header) {
            var subHeader = header.GetSubSectionHeader<OccurrenceFrequenciesSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as OccurrenceFrequenciesSummarySection;
                section.SummarizeUncertain(data.OccurrenceFractions);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
