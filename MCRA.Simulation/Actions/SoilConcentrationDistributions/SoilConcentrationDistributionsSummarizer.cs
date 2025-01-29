using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {
    public enum SoilConcentrationDistributionsSections {
        //No sub-sections
    }
    public class SoilConcentrationDistributionsSummarizer : ActionResultsSummarizerBase<ISoilConcentrationDistributionsActionResult> {

        public override ActionType ActionType => ActionType.SoilConcentrationDistributions;

        public override void Summarize(ActionModuleConfig sectionConfig, ISoilConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SoilConcentrationDistributionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new SoilConcentrationDistributionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.SoilConcentrationDistributions);
            subHeader.SaveSummarySection(section);
        }
    }
}
