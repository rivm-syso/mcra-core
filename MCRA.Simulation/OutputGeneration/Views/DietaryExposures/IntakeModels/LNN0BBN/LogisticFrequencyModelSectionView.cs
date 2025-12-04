using System.Text;
using System.Web;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;

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
            sb.AppendTable(
                Model,
                Model.FrequencyModelRecords,
                "FrequencyModelSummaryTable",
                ViewBag,
                header: true,
                saveCsv: true
            );
        }
    }
}
