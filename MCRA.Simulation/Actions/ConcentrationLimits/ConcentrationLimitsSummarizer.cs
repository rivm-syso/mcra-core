using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConcentrationLimits {
    public enum ConcentrationLimitsSections {
        //No sub-sections
    }
    public class ConcentrationLimitsSummarizer : ActionResultsSummarizerBase<IConcentrationLimitsActionResult> {

        public override ActionType ActionType => ActionType.ConcentrationLimits;

        public override void Summarize(ActionModuleConfig sectionConfig, IConcentrationLimitsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationLimitsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new ConcentrationLimitsDataSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            section.Summarize(data.MaximumConcentrationLimits.Values, data.ConcentrationUnit);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName())
            };
            return result;
        }
    }
}
