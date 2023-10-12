using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PointsOfDeparture {
    public enum PointsOfDepartureSections {
        //No sub-sections
    }
    public sealed class PointsOfDepartureSummarizer : ActionResultsSummarizerBase<IPointsOfDepartureActionResult> {

        public override ActionType ActionType => ActionType.PointsOfDeparture;

        public override void Summarize(ProjectDto project, IPointsOfDepartureActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<PointsOfDepartureSections>(project, ActionType);

            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new PointsOfDepartureSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.PointsOfDeparture.GetDisplayName(), order);
            section.Summarize(
                data.PointsOfDeparture,
                project.UncertaintyAnalysisSettings.DoUncertaintyAnalysis
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
