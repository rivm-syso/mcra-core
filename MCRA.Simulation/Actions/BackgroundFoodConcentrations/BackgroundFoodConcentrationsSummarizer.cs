using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.BackgroundFoodConcentrations {

    public enum BackgroundFoodConcentrationsSections {
        BackgroundFoodConcentrationsSection
    }

    public sealed class BackgroundFoodConcentrationsSummarizer : ActionResultsSummarizerBase<IBackgroundFoodConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.BackgroundFoodConcentrations;

        public override void Summarize(
            ActionModuleConfig outputConfig,
            IBackgroundFoodConcentrationsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<BackgroundFoodConcentrationsSections>(outputConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            if (data.BackgroundFoodConcentrations != null && outputSettings.ShouldSummarize(BackgroundFoodConcentrationsSections.BackgroundFoodConcentrationsSection)) {
                summarizeBackgroundFoodConcentrations(data, subHeader, subOrder++);
            }
        }

        private void summarizeBackgroundFoodConcentrations(ActionData data, SectionHeader header, int order) {
            var section = new BackgroundFoodConcentrationsSummarySection() {
                SectionLabel = getSectionLabel(BackgroundFoodConcentrationsSections.BackgroundFoodConcentrationsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Background food concentrations",
                order++
            );
            section.Summarize(data.BackgroundFoodConcentrations);
            subHeader.SaveSummarySection(section);
        }
    }
}
