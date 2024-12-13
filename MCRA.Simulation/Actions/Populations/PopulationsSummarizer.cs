using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Populations {
    public enum PopulationsSections {
        //No sub-sections
    }
    public class PopulationsSummarizer : ActionResultsSummarizerBase<IPopulationsActionResult> {

        public override ActionType ActionType => ActionType.Populations;

        public override void Summarize(ActionModuleConfig sectionConfig, IPopulationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<PopulationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            if (data.SelectedPopulation != null) {
                var section = new PopulationsSummarySection() {
                    SectionLabel = ActionType.ToString()
                };
                var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
                subHeader.Units = collectUnits(data);
                section.Summarize(data.SelectedPopulation);
                subHeader.SaveSummarySection(section);
            }
        }
        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()),
            };
            return result;
        }
    }
}
