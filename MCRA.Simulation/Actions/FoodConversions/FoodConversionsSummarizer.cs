using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FoodConversions {
    public enum FoodConversionsSections {
        FoodConversionsSection,
        FailedFoodConversionsSection,
        ConversionSummarySection,
        FoodsNotFoundTDSFoodSampleCompositionsSection,
        ModelledFoodNoConversionSection
    }

    public sealed class FoodConversionsSummarizer : ActionResultsSummarizerBase<FoodConversionActionResult> {
        public override ActionType ActionType => ActionType.FoodConversions;

        public override void Summarize(ProjectDto project, FoodConversionActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<FoodConversionsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var conversionSummary = new ConversionSummary() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(conversionSummary, ActionType.GetDisplayName(), order);

            int subOrder = 0;

            if (outputSettings.ShouldSummarize(FoodConversionsSections.ConversionSummarySection)) {
                summarizeConversionSummary(result, data, subHeader, subOrder++);
            }

            if (outputSettings.ShouldSummarize(FoodConversionsSections.FoodConversionsSection)) {
                summarizeFoodConversion(result, data, subHeader, subOrder++);
            }

            if (project.AssessmentSettings.TotalDietStudy && result.FailedFoodConversionResults.Any()
                && outputSettings.ShouldSummarize(FoodConversionsSections.FoodsNotFoundTDSFoodSampleCompositionsSection)) {
                summarizeFoodsNotFoundTDSFoodSampleComposition(result, subHeader, subOrder++);
            }

            if (outputSettings.ShouldSummarize(FoodConversionsSections.FailedFoodConversionsSection)) {
                var matchedFoodsAsEaten = result.FoodConversionResults.Select(r => r.FoodAsEaten).ToHashSet();
                if (result.FailedFoodConversionResults.Any()) {
                    summarizeUnmatchedFoodsAsEaten(project, result, subHeader, subOrder++);
                }
            }

            if (outputSettings.ShouldSummarize(FoodConversionsSections.ModelledFoodNoConversionSection)) {
                var matchedModelledFoods = result.FoodConversionResults.Select(r => r.FoodAsMeasured).ToHashSet();
                if (data.ModelledFoods.Any(r => !matchedModelledFoods.Contains(r))) {
                    summarizeUnmatchedModelledFoods(result, data, subHeader, subOrder++);
                }
            }
        }

        private void summarizeConversionSummary(FoodConversionActionResult result, ActionData data, SectionHeader header, int order) {
            var section = new ConversionSummarySection() {
                SectionLabel = getSectionLabel(FoodConversionsSections.ConversionSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Conversion summary",
                order
            );
            section.Summarize(result.FoodConversionResults, result.FailedFoodConversionResults, data.ModelledFoods);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeFoodConversion(FoodConversionActionResult result, ActionData data, SectionHeader header, int order) {
            var conversionsSection = new ConversionsSection() {
                SectionLabel = getSectionLabel(FoodConversionsSections.FoodConversionsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                conversionsSection,
                "Food conversions",
                order
            );
            conversionsSection.Summarize(result.FoodConversionResults, data.ActiveSubstances);
            subHeader.SaveSummarySection(conversionsSection);
        }

        private void summarizeFoodsNotFoundTDSFoodSampleComposition(FoodConversionActionResult result, SectionHeader header, int order) {
            var section = new UnmatchedTdsFoodsSection() {
                SectionLabel = getSectionLabel(FoodConversionsSections.FoodsNotFoundTDSFoodSampleCompositionsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Foods not found in TDS food-sample compositions",
                order
            );
            section.Summarize(result.FailedFoodConversionResults);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeUnmatchedFoodsAsEaten(ProjectDto project, FoodConversionActionResult result, SectionHeader header, int order) {
            var section = new UnMatchedFoodAsEatenSummarySection() {
                SectionLabel = getSectionLabel(FoodConversionsSections.FailedFoodConversionsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Unmatched foods as eaten",
                order
            );
            var matchedFoodsAsEaten = result.FoodConversionResults.Select(r => r.FoodAsEaten).ToHashSet();
            var failedFoodConversionResults = result.FailedFoodConversionResults.Where(c => !matchedFoodsAsEaten.Contains(c.FoodAsEaten)).ToList();
            var substanceIndependent = !project.AssessmentSettings.MultipleSubstances || project.ConversionSettings.SubstanceIndependent;
            section.Summarize(failedFoodConversionResults, substanceIndependent);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeUnmatchedModelledFoods(FoodConversionActionResult result, ActionData data, SectionHeader header, int order) {
            var section = new UnMatchedModelledFoodSummarySection() {
                SectionLabel = getSectionLabel(FoodConversionsSections.ModelledFoodNoConversionSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Unmatched modelled foods",
                order
            );
            var matchedModelledFoods = result.FoodConversionResults.Select(r => r.FoodAsMeasured).ToHashSet();
            var unmatchedModelledFoods = data.ModelledFoods.Where(r => !matchedModelledFoods.Contains(r)).ToList();
            section.Summarize(unmatchedModelledFoods);
            subHeader.SaveSummarySection(section);
        }
    }
}
