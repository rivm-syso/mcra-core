using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ApprovalBySubstanceSummarySectionView : SectionView<ApprovalBySubstanceSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.UnspecifiedApprovals > 0) {
                sb.AppendDescriptionParagraph(
                    $"Approval status not specified for {Model.UnspecifiedApprovals} substance. These substances are considered NOT approved."
                );
            }

            if (Model.Records.Any()) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SubstanceApprovalsDataTable",
                    ViewBag,
                    caption: Model.UnspecifiedApprovals == 0 
                        ? "Substance approvals."
                        : $"Substance approvals specified in the data. Approval status not specified for {Model.UnspecifiedApprovals} substance. These substances are considered NOT approved.",
                    header: true,
                    saveCsv: true
                );
            } else {
                sb.AppendParagraph("No substance approvals found for any combination of measured substance.", "warning");
            }
        }
    }
}
