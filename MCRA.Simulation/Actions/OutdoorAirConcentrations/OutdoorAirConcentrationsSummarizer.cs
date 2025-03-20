using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.OutdoorAirConcentrations {
    public enum OutdoorAirConcentrationsSections {
        //No sub-sections
    }
    public class OutdoorAirConcentrationsSummarizer : ActionResultsSummarizerBase<IOutdoorAirConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.OutdoorAirConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, IOutdoorAirConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<OutdoorAirConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new OutdoorAirConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.OutdoorAirConcentrations);
            subHeader.SaveSummarySection(section);
        }
    }
}
