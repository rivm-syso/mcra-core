using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DustConcentrations {
    public enum DustConcentrationsSections {
        //No sub-sections
    }
    public class DustConcentrationsSummarizer : ActionResultsSummarizerBase<IDustConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.DustConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, IDustConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DustConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new DustConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);

            section.Summarize(
                data.DustConcentrations,
                data.DustConcentrationUnit,
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
