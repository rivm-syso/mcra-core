using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SubstanceAuthorisations {
    public enum SubstanceAuthorisationsSections {
        //No sub-sections
    }
    public sealed class SubstanceAuthorisationsSummarizer : ActionResultsSummarizerBase<ISubstanceAuthorisationsActionResult> {

        public override ActionType ActionType => ActionType.SubstanceAuthorisations;

        public override void Summarize(ProjectDto project, ISubstanceAuthorisationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SubstanceAuthorisationsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SubstanceAuthorisationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.SaveSummarySection(section);
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
