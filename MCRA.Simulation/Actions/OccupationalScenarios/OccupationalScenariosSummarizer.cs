using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.OccupationalScenarios {
    public enum OccupationalScenariosSections {
        OccupationalTasksDataSection,
        OccupationalTaskExposuresDataSection,
        OccupationalScenarioTasksDataSection,
    }

    public class OccupationalScenariosSummarizer : ActionResultsSummarizerBase<IOccupationalScenariosActionResult> {

        public override ActionType ActionType => ActionType.OccupationalScenarios;

        public override void Summarize(ActionModuleConfig sectionConfig, IOccupationalScenariosActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<OccupationalScenariosSections>(sectionConfig, ActionType);
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
    }
}

