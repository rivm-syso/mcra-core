using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SubstanceAuthorisations {
    public enum SubstanceAuthorisationsSections {
        //No sub-sections
    }
    public sealed class SubstanceAuthorisationsSummarizer : ActionResultsSummarizerBase<ISubstanceAuthorisationsActionResult> {

        public override ActionType ActionType => ActionType.SubstanceAuthorisations;

        public override void Summarize(ActionModuleConfig sectionConfig, ISubstanceAuthorisationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SubstanceAuthorisationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            summarizeByFoodSubstance(
                data.SubstanceAuthorisations,
                subHeader,
                subOrder++
            );
        }

        private static void summarizeByFoodSubstance(
                IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
                SectionHeader subHeader,
                int subOrder
            ) {
            var subSection = new AuthorisationsByFoodSubstanceSummarySection();
            var sub2Header = subHeader.AddSubSectionHeaderFor(subSection, "Substance authorisations by food and substance", subOrder++);
            subSection.Summarize(substanceAuthorisations);
            sub2Header.SaveSummarySection(subSection);
        }
    }
}
