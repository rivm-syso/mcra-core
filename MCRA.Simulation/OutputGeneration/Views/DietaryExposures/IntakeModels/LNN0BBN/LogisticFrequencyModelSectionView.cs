using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;
using System.Web;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class LogisticFrequencyModelSectionView : SectionView<LogisticFrequencyModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            sb.AppendParagraph("Linear model at logit scale. phi = overdispersion factor, quantifies variation in frequency between individuals");
            if (!string.IsNullOrEmpty(Model.Message.GetDisplayName())) {
                sb.Append($"<ul><li class='warning'>{HttpUtility.HtmlEncode(Model.Message.GetDisplayName())}</li></ul>");
            }
            if (Model.LikelihoodRatioTestResults != null && Model.LikelihoodRatioTestResults.PValue.Count > 0) {
                sb.Append("<div>");
                sb.AppendParagraph("Automatic model selection results");
                sb.Append("<table>");
                sb.AppendHeaderRow("Degree of polynomial", "-2 * Loglikelihood", "Degrees of freedom", "Likelihood ratio", "Df", "Probability");
                for (int i = 0; i < Model.LikelihoodRatioTestResults.PValue.Count; i++) {
                    sb.AppendTableRow(
                        Model.LikelihoodRatioTestResults.DfPolynomial[i],
                        Model.LikelihoodRatioTestResults.LogLikelihood[i].ToString("F2"),
                        Model.LikelihoodRatioTestResults.DegreesOfFreedom[i],
                        Model.LikelihoodRatioTestResults.DeltaChi[i].ToString("G2"),
                        Model.LikelihoodRatioTestResults.DeltaDf[i],
                        Model.LikelihoodRatioTestResults.PValue[i].ToString("G2"));
                }
                sb.AppendTableRow(
                    Model.LikelihoodRatioTestResults.DfPolynomial.Last(),
                    Model.LikelihoodRatioTestResults.LogLikelihood.Last().ToString("F2"),
                    Model.LikelihoodRatioTestResults.DegreesOfFreedom.Last(),
                    "-", "-", "-");

                sb.Append("</table>");
                sb.Append("</div>");
            }

            sb.Append("<table>");
            sb.AppendHeaderRow("Parameter", "Estimate", "s.e.", "t-value");
            sb.AppendTableRow(
                Model.VarianceEstimates.ParameterName,
                Model.VarianceEstimates.Estimate.ToString("G2"),
                Model.VarianceEstimates.StandardError.ToString("G2"),
                Model.VarianceEstimates.TValue.ToString("G2"));

            foreach (var item in Model.FrequencyModelEstimates) {
                sb.AppendTableRow(
                    item.ParameterName,
                    item.Estimate.ToString("G2"),
                    item.StandardError.ToString("F2"),
                    item.TValue.ToString("G2"));
            }
            sb.AppendTableRow("degrees of freedom", Model.DegreesOfFreedom.ToString("F0"), "", "");
            sb.AppendTableRow("-2*loglikelihood", Model._2LogLikelihood.ToString("F2"), "", "");

            sb.Append("</table>");
        }
    }
}
