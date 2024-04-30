using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.NonDietaryExposures {
    public enum NonDietaryExposuresSections {
        //No sub-sections
    }
    public sealed class NonDietaryExposuresSummarizer : ActionResultsSummarizerBase<INonDietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.NonDietaryExposures;

        public override void Summarize(ActionModuleConfig sectionConfig, INonDietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<NonDietaryExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new NonDietaryInputDataSection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(
                data.NonDietaryExposures,
                data.ActiveSubstances
            );
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("NonDietaryExposureUnit", data.NonDietaryExposureUnit.GetShortDisplayName())
            };
            return result;
        }
    }
}
