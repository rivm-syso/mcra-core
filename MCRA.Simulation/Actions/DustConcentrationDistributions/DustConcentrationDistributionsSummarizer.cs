using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {
    public enum DustConcentrationDistributionsSections {
        //No sub-sections
    }
    public class DustConcentrationDistributionsSummarizer : ActionResultsSummarizerBase<IDustConcentrationDistributionsActionResult> {

        public override ActionType ActionType => ActionType.DustConcentrationDistributions;

        public override void Summarize(ActionModuleConfig sectionConfig, IDustConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DustConcentrationDistributionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new DustConcentrationDistributionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);

            section.Summarize(
                data.DustConcentrationDistributions,
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
