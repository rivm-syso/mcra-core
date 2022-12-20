using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownIndividualsSectionView : SectionView<DietaryAcuteDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.Append("<table class=\"sortable\"><thead>");
            sb.AppendHeaderRow(
                "Individual ID",
                $"Body weight ({ViewBag.GetUnit("BodyWeightUnit")})",
                "Day",
                $"Dietary exposure ({ViewBag.GetUnit("IntakeUnit")})",
                "Sampling weight"
            );
            sb.Append("</thead><tbody>");

            foreach (var item in Model.DrillDownSummaryRecords) {
                sb.AppendTableRow(
                    item.IndividualCode,
                    item.BodyWeight,
                    item.Day,
                    item.DietaryIntakePerMassUnit.ToString("G3"),
                    item.SamplingWeight.ToString("F2")
                );
            }
            sb.Append("</tbody></table>");
        }
    }
}
