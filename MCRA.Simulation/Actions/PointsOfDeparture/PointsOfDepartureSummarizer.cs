using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.PointsOfDeparture {
    public enum PointsOfDepartureSections {
        //No sub-sections
    }
    public sealed class PointsOfDepartureSummarizer : ActionModuleResultsSummarizer<PointsOfDepartureModuleConfig, IPointsOfDepartureActionResult> {

        public PointsOfDepartureSummarizer(PointsOfDepartureModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, IPointsOfDepartureActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<PointsOfDepartureSections>(sectionConfig, ActionType);

            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new PointsOfDepartureSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.PointsOfDeparture.GetDisplayName(), order);
            section.Summarize(
                data.PointsOfDeparture,
                _configuration.DoUncertaintyAnalysis
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
