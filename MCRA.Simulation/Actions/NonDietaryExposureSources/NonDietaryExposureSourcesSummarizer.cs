using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.NonDietaryExposureSources {
    public enum NonDietaryExposureSourcesSections {
        NonDietaryExposureSourcesSection
    }
    public sealed class NonDietaryExposureSourcesSummarizer : ActionResultsSummarizerBase<INonDietaryExposureSourcesActionResult> {

        public override ActionType ActionType => ActionType.NonDietaryExposureSources;

        public override void Summarize(ActionModuleConfig sectionConfig, INonDietaryExposureSourcesActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<NonDietaryExposureSourcesSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            // Summarize non-dietary exposure sources catalogue
            if (outputSettings.ShouldSummarize(NonDietaryExposureSourcesSections.NonDietaryExposureSourcesSection)) {
                summarizeNonDietaryExposureSources(data, subHeader, subOrder++);
            }
        }

        private void summarizeNonDietaryExposureSources(ActionData data, SectionHeader header, int order) {
            var section = new NonDietaryExposureSourcesSummarySection {
                SectionLabel = getSectionLabel(NonDietaryExposureSourcesSections.NonDietaryExposureSourcesSection)
            };

            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Non-dietary exposure sources",
                order
            );

            section.Summarize(data.NonDietaryExposureSources);
            subHeader.SaveSummarySection(section);
        }
    }
}
