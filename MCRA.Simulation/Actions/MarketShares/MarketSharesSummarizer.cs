using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.MarketShares {
    public enum MarketSharesSections { }
    public class MarketSharesSummarizer : ActionResultsSummarizerBase<IMarketSharesActionResult> {

        public override ActionType ActionType => ActionType.MarketShares;

        public override void Summarize(ProjectDto project, IMarketSharesActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<MarketSharesSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            if (data.MarketShares.Count > 0) {
                var section = new MarketSharesSummarySection() {
                    SectionLabel = ActionType.ToString()
                };
                var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
                section.Summarize(data.MarketShares);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
