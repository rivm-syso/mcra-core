using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SoilConcentrations {
    public enum SoilConcentrationsSections {
        //No sub-sections
    }
    public class SoilConcentrationsSummarizer : ActionResultsSummarizerBase<ISoilConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.SoilConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, ISoilConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SoilConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new SoilConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);

            section.Summarize(
                data.SoilConcentrations,
                data.SoilConcentrationUnit,
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
