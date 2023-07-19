using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Populations {
    public enum PopulationsSections {
        //No sub-sections
    }
    public class PopulationsSummarizer : ActionResultsSummarizerBase<IPopulationsActionResult> {

        public override ActionType ActionType => ActionType.Populations;

        public override void Summarize(ProjectDto project, IPopulationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<PopulationsSections>(project, ActionType);
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
                new ActionSummaryUnitRecord("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()),
            };
            return result;
        }
    }
}
