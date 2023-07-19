using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SubstanceConversions {
    public enum SubstanceConversionsSections {
        //No sub-sections
    }
    public class SubstanceConversionsSummarizer : ActionResultsSummarizerBase<ISubstanceConversionsActionResult> {

        public override ActionType ActionType => ActionType.SubstanceConversions;

        public override void Summarize(ProjectDto project, ISubstanceConversionsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SubstanceConversionsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SubstanceConversionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.SubstanceConversions, data.DeterministicSubstanceConversionFactors);
            var subOrder = 0;
            if (data.SubstanceConversions?.Any() ?? false) {
                summarizeSubstanceTranslations(data.SubstanceConversions, subHeader, subOrder++);
                summarizeNominalSubstanceTranslations(data.SubstanceConversions, subHeader, subOrder++);
            }
            subHeader.SaveSummarySection(section);
        }

        private static void summarizeSubstanceTranslations(
            ICollection<SubstanceConversion> substanceConversions,
            SectionHeader subHeader,
            int subOrder
        ) {
            var subSection = new SubstanceConversionsDataSection();
            var sub2Header = subHeader.AddSubSectionHeaderFor(subSection, "Substance conversion rules", subOrder++);
            subSection.Summarize(substanceConversions);
            sub2Header.SaveSummarySection(subSection);
        }

        private static void summarizeNominalSubstanceTranslations(
            ICollection<SubstanceConversion> substanceConversions,
            SectionHeader subHeader,
            int subOrder
        ) {
            var subSection = new NominalTranslationProportionsSection();
            var sub2Header = subHeader.AddSubSectionHeaderFor(subSection, "Nominal substance conversions", subOrder++);
            subSection.Summarize(substanceConversions);
            sub2Header.SaveSummarySection(subSection);
        }
    }
}
