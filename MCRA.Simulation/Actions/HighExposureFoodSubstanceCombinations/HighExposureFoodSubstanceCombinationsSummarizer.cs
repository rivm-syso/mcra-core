using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations {
    public enum HighExposureFoodSubstanceCombinationsSections {
        //No sub-sections
    }
    public class HighExposureFoodSubstanceCombinationsSummarizer : ActionResultsSummarizerBase<HighExposureFoodSubstanceCombinationsActionResult> {

        public override ActionType ActionType => ActionType.HighExposureFoodSubstanceCombinations;

        public override void Summarize(ProjectDto project, HighExposureFoodSubstanceCombinationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<HighExposureFoodSubstanceCombinationsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ScreeningSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(
                actionResult.ScreeningResult,
                project.ScreeningSettings.CriticalExposurePercentage,
                project.ScreeningSettings.CumulativeSelectionPercentage,
                project.ScreeningSettings.ImportanceLor
            );
            subHeader.Units = collectUnits(project, data);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("IntakeUnit", data.DietaryExposureUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix))
            };
            return result;
        }
    }
}
