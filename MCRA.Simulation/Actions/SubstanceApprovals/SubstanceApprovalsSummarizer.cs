using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SubstanceApprovals {
    public enum SubstanceApprovalsSections {
        //No sub-sections
    }
    public sealed class SubstanceApprovalsSummarizer : ActionResultsSummarizerBase<ISubstanceApprovalsActionResult> {

        public override ActionType ActionType => ActionType.SubstanceApprovals;

        public override void Summarize(ActionModuleConfig sectionConfig, ISubstanceApprovalsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SubstanceApprovalsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            summarizeBySubstance(
                data.SubstanceApprovals,
                data.AllCompounds,
                subHeader,
                subOrder++
            );
        }

        private static void summarizeBySubstance(
            IDictionary<Compound, SubstanceApproval> substanceApprovals,
            ICollection<Compound> substances,
            SectionHeader subHeader,
            int subOrder
        ) {
            var subSection = new ApprovalBySubstanceSummarySection();
            var sub2Header = subHeader.AddSubSectionHeaderFor(subSection, "Substance approvals by substance", subOrder++);
            subSection.Summarize(substanceApprovals, substances);
            sub2Header.SaveSummarySection(subSection);
        }
    }
}
