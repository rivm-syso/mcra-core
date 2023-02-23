using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PredictedHealthEffectSectionView : SectionView<PredictedHealthEffectSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Percentiles.Any(r => !double.IsNaN(r.MedianUncertainty) && r.MedianUncertainty > 0);

            //Render HTML
            sb.Append("<br/>");
            sb.AppendParagraph($"Model is:  {Model.DoseResponseModelEquation} with values {Model.ParameterValues}: ");
            sb.AppendBase64Image(new PredictedHealthEffectChartCreator(Model, ViewBag.GetUnit("ConcentrationUnit")), "PredictedHealthEffectChart");
            sb.AppendBase64Image(new PredictedHealthEffectCumulativeChartCreator(Model, ViewBag.GetUnit("ConcentrationUnit")), "PredictedHealthEffectCumulativeChart");

            sb.Append("<table><thead>");

            var row = new ArrayList {
                "Percentage<br/>",
                $"Effects (animal dose)<br/>({ViewBag.GetUnit("ConcentrationUnit").ToHtml()})"
            };
            if (isUncertainty) {
                row.Add($"Lower Bound (p{Model.UncertaintyLowerLimit})");
                row.Add($"Upper Bound (p{Model.UncertaintyUpperLimit})");
            }
            sb.AppendRawHeaderRow(row);

            sb.Append("</thead><tbody>");
            foreach (var p in Model.Percentiles) {
                var percentage = p.XValue / 100;
                row = new ArrayList {
                    percentage.ToString("P2"),
                    p.ReferenceValue.ToString("G4")
                };
                if (isUncertainty) {
                    row.Add(p.Percentile(Model.UncertaintyLowerLimit).ToString("G4"));
                    row.Add(p.Percentile(Model.UncertaintyUpperLimit).ToString("G4"));
                }
                sb.AppendTableRow(row);
            }
            sb.Append("</tbody></table>");
        }
    }
}
