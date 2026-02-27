using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.OccupationalTaskExposures {
    public enum OccupationalTaskExposuresSections {
        OccupationalTaskExposuresSection,
    }

    public class OccupationalTaskExposuresSummarizer : ActionResultsSummarizerBase<IOccupationalTaskExposuresActionResult> {

        public override ActionType ActionType => ActionType.OccupationalTaskExposures;

        public override void Summarize(ActionModuleConfig sectionConfig, IOccupationalTaskExposuresActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<OccupationalTaskExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            // Summarize occupational task exposures.
            if (data.OccupationalTaskExposures != null) {
                summarizeOccupationalTaskExposures(data, subHeader, subOrder++);
            }

            // Summarize occupational task exposures for combination of scenario, tasks, determinants and route.
            if (data.OccupationalTaskExposures != null) {
                summarizeOccupationalTaskExposuresPerRoute(data, subHeader, subOrder++);
            }
        }

        private void summarizeOccupationalTaskExposures(
          ActionData data,
          SectionHeader header,
          int order
        ) {
            var section = new OccupationalTaskExposuresDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Occupational task exposures", order);
            section.Summarize(
                data.OccupationalTaskExposures
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeOccupationalTaskExposuresPerRoute(
          ActionData data,
          SectionHeader header,
          int order
        ) {
            var section = new OccupationalTaskExposuresPerRouteSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Occupational scenarios and tasks with exposures", order);
            section.Summarize(
                data.OccupationalTaskExposures,
                data.OccupationalScenarios,
                data.ActiveSubstances ?? data.AllCompounds
            );
            subHeader.SaveSummarySection(section);
        }
    }
}

