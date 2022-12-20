using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EquivalentAnimalDoseSectionView : SectionView<EquivalentAnimalDoseSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Percentiles.Any(r => !double.IsNaN(r.MedianUncertainty) && r.MedianUncertainty > 0);

            //Render HTML
            sb.AppendParagraph($"Mean (with standard deviation within parenthesis): {Model.Mean:G4} ({ViewBag.GetUnit("ConcentrationUnit")}) ({Model.StandardDeviation:G4})");
            sb.AppendBase64Image(new EquivalentAnimalDoseChartCreator(Model, ViewBag.GetUnit("ConcentrationUnit")), "EquivalentAnimalDoseChart");
            sb.AppendBase64Image(new EquivalentAnimalDoseCumulativeChartCreator(Model, ViewBag.GetUnit("ConcentrationUnit")), "EquivalentAnimalDoseCumulativeChart");
            sb.AppendParagraph($"Reference: {Model.Reference.Name} ");

            sb.Append("<table><thead>");
            var row = new ArrayList {
                "Percentage<br/>",
                $"Equivalent animal dose<br/>({ViewBag.GetUnit("ConcentrationUnit")})"
            };
            if (isUncertainty) {
                row.Add($"Lower Bound ({$"p{Model.UncertaintyLowerLimit}"})");
                row.Add($"Upper Bound ({$"p{Model.UncertaintyUpperLimit}"})");
            }
            sb.AppendRawHeaderRow(row.ToArray());

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
                sb.AppendTableRow(row.ToArray());
            }
            sb.Append("</tbody></table>");
        }
    }
}
