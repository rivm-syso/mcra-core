using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.OccupationalTaskExposures {
    public enum OccupationalTaskExposuresSections {
        OccupationalTasksDataSection,
        OccupationalTaskExposuresDataSection,
        OccupationalScenarioTasksDataSection,
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

            // Summarize occupational scenarios.
            if (data.OccupationalScenarios != null) {
                summarizeOccupationalScenarios(data, subHeader, subOrder++);
            }

            // Summarize Occupational scenario tasks.
            if (data.OccupationalScenarios != null) {
                summarizeOccupationalScenarioTasks(data, subHeader, subOrder++);
            }

            // Summarize Occupational tasks.
            if (data.OccupationalTasks != null) {
                summarizeOccupationalTasks(data, subHeader, subOrder++);
            }

            // Summarize occupational task exposures.
            if (data.OccupationalTaskExposures != null) {
                summarizeOccupationalTaskExposures(data, subHeader, subOrder++);
            }
        }

        private void summarizeOccupationalScenarios(
           ActionData data,
           SectionHeader header,
           int order
        ) {
            var section = new OccupationalScenariosDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Occupational scenarios", order);
            section.Summarize(
                data.OccupationalScenarios
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeOccupationalTasks(
           ActionData data,
           SectionHeader header,
           int order
        ) {
            var section = new OccupationalTasksDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Occupational tasks", order);
            section.Summarize(
                data.OccupationalTasks
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeOccupationalScenarioTasks(
           ActionData data,
           SectionHeader header,
           int order
        ) {
            var section = new OccupationalScenarioTasksDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Occupational scenario tasks", order);
            section.Summarize(
                data.OccupationalScenarios
            );
            subHeader.SaveSummarySection(section);
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
    }
}

