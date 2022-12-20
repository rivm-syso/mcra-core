using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;
using System.Web;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class LNNModelResultsSectionView : SectionView<LNNModelResultsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            if (Model.FallBackModel == IntakeModelType.LNN0) {
                sb.AppendParagraph("LNN with correlation cannot be fitted because exposure is not incidental. Fit LNN without correlation instead");
            } else {

                sb.AppendParagraph("LNN uses initial estimates based on the logistic normal frequency and normal amounts model without correlation");
                if (!string.IsNullOrEmpty(Model.Message.GetDisplayName())) {
                    sb.Append($"<ul><li class='warning'>{HttpUtility.HtmlEncode(Model.Message.GetDisplayName())}</li></ul>");
                }
                sb.Append("<div>");
                sb.AppendParagraph("Logistic normal frequency model");
                sb.Append("<table>");
                sb.AppendHeaderRow("Parameter", "Estimate", "s.e.", "t-value");
                sb.AppendTableRow(
                    Model.VarianceEstimates.ParameterName,
                    Model.VarianceEstimates.Estimate.ToString("F2"),
                    Model.VarianceEstimates.StandardError.ToString("F2"),
                    Model.VarianceEstimates.TValue.ToString("F2")
                );
                foreach (var item in Model.FrequencyModelEstimates) {
                    sb.AppendTableRow(
                        item.ParameterName,
                        item.Estimate.ToString("F2"),
                        item.StandardError.ToString("F2"),
                        item.TValue.ToString("F2")
                    );
                }
                sb.AppendTableRow("degrees of freedom", Model.DegreesOfFreedom.ToString("F0"), "", "");
                sb.AppendTableRow("-2*loglikelihood", Model._2LogLikelihood.ToString("F2"), "", "");
                sb.Append("</table>");
                sb.Append("</div>");

                sb.Append("<div>");
                sb.AppendParagraph("Normal amounts model");
                sb.Append("<table>");
                sb.AppendHeaderRow("Parameter", "Estimate", "s.e.", "t-value");
                sb.AppendTableRow("transformation power", Model.Power.ToString("F2"), "", "");
                foreach (var item in Model.AmountsModelEstimates) {
                    sb.AppendTableRow(
                        item.ParameterName,
                        item.Estimate.ToString("F2"),
                        item.StandardError.ToString("F2"),
                        item.TValue.ToString("F2")
                    );
                }
                sb.AppendTableRow("variance between individuals", Model.VarianceBetween.ToString("F4"), "", "");
                sb.AppendTableRow("variance within individuals", Model.VarianceWithin.ToString("F4"), "", "");
                sb.AppendTableRow(
                    Model.CorrelationEstimates.ParameterName,
                    Model.CorrelationEstimates.Estimate.ToString("F2"),
                    Model.CorrelationEstimates.StandardError.ToString("F2"),
                    "");

                sb.AppendTableRow("-2*loglikelihood", Model._2LogLikelihood.ToString("F2"), "", "");
                sb.Append("</table>");
                sb.Append("</div>");
            }
        }
    }
}
