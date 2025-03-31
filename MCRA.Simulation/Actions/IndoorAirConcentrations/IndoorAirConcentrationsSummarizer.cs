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
            subHeader.Units = collectUnits(data, sectionConfig);

            section.Summarize(
                data.IndoorAirConcentrations,
                sectionConfig.VariabilityLowerPercentage,
                sectionConfig.VariabilityUpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}")
            };
            return result;
        }
    }
}
