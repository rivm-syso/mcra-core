using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.IndoorAirConcentrations {
    public enum IndoorAirConcentrationsSections {
        //No sub-sections
    }
    public class IndoorAirConcentrationsSummarizer : ActionResultsSummarizerBase<IIndoorAirConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.IndoorAirConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, IIndoorAirConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<IndoorAirConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new IndoorAirConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.IndoorAirConcentrations);
            subHeader.SaveSummarySection(section);
        }
    }
}
