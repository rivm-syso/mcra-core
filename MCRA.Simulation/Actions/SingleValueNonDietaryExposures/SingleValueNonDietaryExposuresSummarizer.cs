using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {
    public enum SingleValueNonDietaryExposures {
        ExposureScenarios,
        ExposureEstimates,
        ExposureDeterminantCombinations
    }
    public sealed class SingleValueNonDietaryExposuresSummarizer : ActionResultsSummarizerBase<SingleValueNonDietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.SingleValueNonDietaryExposures;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            SingleValueNonDietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueNonDietaryExposures>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SingleValueNonDietaryDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);

            var subOrder = 0;
            if (outputSettings.ShouldSummarize(SingleValueNonDietaryExposures.ExposureScenarios)) {
                summarizeExposureScenarios(
                    data.SingleValueNonDietaryExposureScenarios,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(SingleValueNonDietaryExposures.ExposureEstimates)) {
                summarizeExposureEstimates(
                    data.SingleValueNonDietaryExposureEstimates,                    
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(SingleValueNonDietaryExposures.ExposureDeterminantCombinations)) {
                summarizeExposureDeterminantCombinations(
                    data.SingleValueNonDietaryExposureDeterminantCombinations,
                    subHeader,
                    subOrder++
                );
            }

            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            // NOTE: exposure unit is defined per scenario. Here it is assumed that the exposure unit
            //       is the same for all scenarios and we take the first one. Later, different units should be
            //       supported in the output (or rescale the exposure values to one unit).
            var firstScenario = data.SingleValueNonDietaryExposureScenarios.FirstOrDefault();
            var result = new List<ActionSummaryUnitRecord> {
                new ("ExposureUnit", firstScenario.Value.ExposureUnit.GetShortDisplayName()),
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
           SectionHeader header,
           int order
        ) {
            var section = new SingleValueNonDietaryExposuresSection() {
                SectionLabel = getSectionLabel(SingleValueNonDietaryExposures.ExposureEstimates)
            };

            section.Summarize(
                exposureEstimates
                );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Overview of exposure estimates",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeExposureDeterminantCombinations(           
           IDictionary<string, ExposureDeterminantCombination> exposureDeterminantCombinations,
           SectionHeader header,
           int order
        ) {
            var section = new SingleValueNonDietaryExposureDeterminantCombinationsSection() {
                SectionLabel = getSectionLabel(SingleValueNonDietaryExposures.ExposureEstimates)
            };

            section.Summarize(                
                exposureDeterminantCombinations
                );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Overview of exposure determinant combinations",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
