using System.Text;
using System.Web;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class LNNModelResultsSectionView : SectionView<LNNModelResultsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.FallBackModel == IntakeModelType.LNN0) {
                sb.AppendWarning("LNN with correlation cannot be fitted because exposure is not incidental. Fit LNN without correlation instead");
            } else {
                sb.AppendParagraph("LNN uses initial estimates based on the logistic normal frequency and normal amounts model without correlation");
                sb.Append($"<ul><li class='warning'>{HttpUtility.HtmlEncode(Model.Message.GetDisplayName())}</li></ul>");
                sb.AppendTable(
                    Model,
                    Model.FrequencyModelFitSummaryRecords,
                    "LNNFrequencyModelSummaryTable",
                    ViewBag,
                    header: true,
                    caption: "Logistic normal frequency model.",
                    saveCsv: true
                );
                sb.AppendTable(
                    Model,
                    Model.AmountModelFitSummaryRecords,
                    "LNNAmountModelSummaryTable",
                    ViewBag,
                    header: true,
                    caption: "Normal amounts model.",
                    saveCsv: true
               );
            }
        }
    }
}