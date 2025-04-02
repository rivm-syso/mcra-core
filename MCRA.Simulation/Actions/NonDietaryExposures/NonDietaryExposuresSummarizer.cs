using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.NonDietaryExposures {
    public enum NonDietaryExposuresSections {
        NonDietaryExposuresSection,
    }
    public sealed class NonDietaryExposuresSummarizer : ActionResultsSummarizerBase<INonDietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.NonDietaryExposures;

        public override void Summarize(ActionModuleConfig sectionConfig, INonDietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<NonDietaryExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new NonDietaryExposuresSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            if (outputSettings.ShouldSummarize(NonDietaryExposuresSections.NonDietaryExposuresSection)) {
                section.Summarize(
                    data.NonDietaryExposures,
                    data.ActiveSubstances,
                    sectionConfig.VariabilityLowerPercentage,
                    sectionConfig.VariabilityUpperPercentage
                );
            }
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}")
            };
            return result;
        }
    }
}
