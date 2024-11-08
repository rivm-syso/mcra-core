using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UncertaintyFactorialSectionView : SectionView<UncertaintyFactorialSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            int col = Model.ExplainedVariance.Count;
            int row = Model.Contributions.First().Count;
            int set = Model.Design.First().Count;

            // Contributions uncertainty sources
            sb.Append("<table>");
            sb.Append("<caption>Contributions uncertainty sources (% variance explained within parenthesis)</caption>");
            sb.Append("<thead>");
            var th = new ArrayList { "" };
            for (int c = 0; c < col; c++) {
                th.Add($"p{Model.Percentages[c]:F2} <br/> ({Model.ExplainedVariance[c]:P1})");
            }
            sb.AppendRawHeaderRow(th.ToArray());
            sb.Append("</thead><tbody>");
            for (int r = 0; r < row; r++) {
                var tr = new ArrayList { Model.UncertaintySources[r] };
                for (int c = 0; c < Model.ExplainedVariance.Count; c++) {
                    tr.Add(Model.Contributions[c][r].ToString("P1"));
                }
                sb.AppendTableRow(tr.ToArray());
            }
            sb.Append("</tbody></table>");

            // Regression coefficients
            sb.Append("<table>");
            sb.Append("<caption>Regression coefficients</caption>");
            sb.Append("<thead>");
            th = [""];
            for (int c = 0; c < col; c++) {
                th.Add($"p{Model.Percentages[c]:F2}");
            }
            sb.AppendRawHeaderRow(th.ToArray());
            sb.Append("</thead><tbody>");
            for (int r = 0; r < row; r++) {
                var tr = new ArrayList { Model.UncertaintySources[r] };
                for (int c = 0; c < Model.ExplainedVariance.Count; c++) {
                    tr.Add(Model.RegressionCoefficients[c][r].ToString("G3"));
                }
                sb.AppendTableRow(tr.ToArray());
            }
            sb.Append("</tbody></table>");

            // Variabes and design
            sb.Append("<table>");
            sb.Append("<caption>Variances (responses) and design (uncertainty sources)</caption>");
            sb.Append("<thead>");
            th = [.. Model.ResponseNames, .. Model.UncertaintySources];
            sb.AppendRawHeaderRow(th.ToArray());
            sb.Append("</thead><tbody>");
            for (int s = 0; s < set; s++) {
                var tr = new ArrayList();
                for (int c = 0; c < Model.ExplainedVariance.Count; c++) {
                    tr.Add(Model.Responses[c][s].ToString("G3"));
                }
                for (int c = 0; c < Model.UncertaintySources.Count; c++) {
                    tr.Add(Model.Design[c][s].ToString("G3"));
                }
                sb.AppendTableRow(tr.ToArray());
            }
            sb.Append("</tbody></table>");
        }
    }
}
