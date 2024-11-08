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
            section.Summarize(data.DustConcentrationDistributions);
            subHeader.SaveSummarySection(section);
        }
    }
}
