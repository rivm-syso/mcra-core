using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NormalAmountsModelSectionView : SectionView<NormalAmountsModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            sb.AppendParagraph("Mixed model at transformed scale with random terms for variation between individuals and between days (within individuals)");
            if (Model.LikelihoodRatioTestResults != null && Model.LikelihoodRatioTestResults.PValue.Count > 0) {
                sb.Append("<div id='BBNModelResultsSection'>");
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

            if (Model.Power == 0) {
                sb.AppendTableRow("transformation", "logarithmic (0)", "", "");
            } else if (Model.Power == 1) {
                sb.AppendTableRow("transformation", "identy (1)", "", "");
            } else {
                sb.AppendTableRow("transformation power", Model.Power.ToString("G2"), "", "");
            }

            foreach (var item in Model.AmountsModelEstimates) {
                sb.AppendTableRow(
                    item.ParameterName,
                    item.Estimate.ToString("G2"),
                    item.StandardError.ToString("F2"),
                    item.TValue.ToString("G2"));
            }

            if (Model.IsAcuteCovariateModelling) {
                sb.AppendTableRow("distribution variance", Model.VarianceBetween.ToString("G4"), "", "");
            } else {
                sb.AppendTableRow("variance between individuals", Model.VarianceBetween.ToString("G4"), "", "");
                sb.AppendTableRow("variance within individuals", Model.VarianceWithin.ToString("G4"), "", "");
            }
            sb.AppendTableRow("degrees of freedom", Model.DegreesOfFreedom.ToString("N0"), "", "");
            sb.AppendTableRow("-2*loglikelihood", Model._2LogLikelihood.ToString("F2"), "", "");

            sb.Append("</table>");
        }
    }
}
