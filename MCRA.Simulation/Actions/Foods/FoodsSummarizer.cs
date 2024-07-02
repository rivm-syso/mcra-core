using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Foods {
    public enum FoodsSections {
        FoodsSection,
        ProcessingTypesSection
    }
    public sealed class FoodsSummarizer : ActionResultsSummarizerBase<IFoodsActionResult> {

        public override ActionType ActionType => ActionType.Foods;

        public override void Summarize(ProjectDto project, IFoodsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<FoodsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            // Summarize foods catalogue
            if (outputSettings.ShouldSummarize(FoodsSections.FoodsSection)) {
                summarizeFoods(data, subHeader, subOrder++);
            }

            // Summarize processing types if available
            if ((data.ProcessingTypes?.Any() ?? false) 
                && outputSettings.ShouldSummarize(FoodsSections.ProcessingTypesSection)
            ) {
                summarizeProcessingTypes(data, subHeader, subOrder++);
            }
        }

        private  void summarizeFoods(ActionData data, SectionHeader header, int order) {
            var section = new FoodsSummarySection {
                SectionLabel = getSectionLabel(FoodsSections.FoodsSection)
            };

            var subHeader = header.AddSubSectionHeaderFor(
                section, 
                "Foods", 
                order
            );
            
            section.Summarize(data.AllFoods);
            subHeader.SaveSummarySection(section);
        }

        private  void summarizeProcessingTypes(ActionData data, SectionHeader header, int order) {
            var section = new ProcessingTypesSummarySection {
                SectionLabel = getSectionLabel(FoodsSections.ProcessingTypesSection)
            };

            var subHeader = header.AddSubSectionHeaderFor(
                section, 
                "Processing types",
                order
            );

            section.Summarize(data.ProcessingTypes);
            subHeader.SaveSummarySection(section);
        }
    }
}
