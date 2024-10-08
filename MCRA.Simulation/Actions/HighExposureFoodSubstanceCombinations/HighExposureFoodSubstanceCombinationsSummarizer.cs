using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations {
    public enum HighExposureFoodSubstanceCombinationsSections {
        //No sub-sections
    }
    public class HighExposureFoodSubstanceCombinationsSummarizer : ActionModuleResultsSummarizer<HighExposureFoodSubstanceCombinationsModuleConfig, HighExposureFoodSubstanceCombinationsActionResult> {

        public HighExposureFoodSubstanceCombinationsSummarizer(HighExposureFoodSubstanceCombinationsModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, HighExposureFoodSubstanceCombinationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<HighExposureFoodSubstanceCombinationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ScreeningSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(
                actionResult.ScreeningResult,
                _configuration.CriticalExposurePercentage,
                _configuration.CumulativeSelectionPercentage,
                _configuration.ImportanceLor
            );
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("IntakeUnit", data.DietaryExposureUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix))
            };
            return result;
        }
    }
}
