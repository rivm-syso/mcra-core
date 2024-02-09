using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {
    public enum SingleValueNonDietaryExposures {
        ExposureScenarios,
        ExposureEstimates        
    }
    public sealed class SingleValueNonDietaryExposuresSummarizer : ActionResultsSummarizerBase<SingleValueNonDietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.SingleValueNonDietaryExposures;

        public override void Summarize(
            ProjectDto project,
            SingleValueNonDietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueNonDietaryExposures>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SingleValueNonDietaryDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);

            var subOrder = 0;
            if (outputSettings.ShouldSummarize(SingleValueNonDietaryExposures.ExposureScenarios)) {
                summarizeExposureScenarios (
                    data.SingleValueNonDietaryExposureScenarios,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(SingleValueNonDietaryExposures.ExposureEstimates)) {
                summarizeExposureEstimates(
                    data.SingleValueNonDietaryExposureEstimates,
                    data.SingleValueNonDietaryExposureDeterminantCombinations,
                    subHeader,
                    subOrder++
                );
            }

            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ExposureUnit", "mg/kg bw/day"),
            };
            return result;
        }

        private void summarizeExposureScenarios(
          IDictionary<string, ExposureScenario> exposureScenarios,
          SectionHeader header,
          int order
       ) {
            var section = new SingleValueNonDietaryExposureScenariosSection() {
                SectionLabel = getSectionLabel(SingleValueNonDietaryExposures.ExposureScenarios)
            };

            section.Summarize(
                exposureScenarios
                );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Overview of exposure scenarios",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeExposureEstimates(
           IList<ExposureEstimate> exposureEstimates,
           IDictionary<string, ExposureDeterminantCombination> exposureDeterminantCombinations,
           SectionHeader header,
           int order
        ) {
            var section = new SingleValueNonDietaryExposuresSection() {
                SectionLabel = getSectionLabel(SingleValueNonDietaryExposures.ExposureEstimates)
            };

            section.Summarize(
                exposureEstimates,
                exposureDeterminantCombinations
                );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Overview of exposure estimates",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
