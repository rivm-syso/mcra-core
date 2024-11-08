using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryDrillDownIndividualsSectionView : SectionView<NonDietaryDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var record = Model.DrillDownSummaryRecords.First();

            //Render HTML
            sb.Append("<table class=\"sortable\"><thead>");
            var row = new ArrayList {
                "Individual ID",
                $"Body weight ({ViewBag.GetUnit("BodyWeightUnit")})",
                "Day"
            };
            foreach (var r in record.CorrectedRouteIntakeRecords) {
                row.Add($"Sum {r.Route}({ViewBag.GetUnit("IntakeUnit")})");
            }
            row.Add($"Sum non dietary exposure ({ViewBag.GetUnit("IntakeUnit")})");
            sb.AppendHeaderRow(row.ToArray());

            sb.Append("</thead><tbody>");

            foreach (var item in Model.DrillDownSummaryRecords) {
                row = [
                    item.IndividualCode,
                    item.BodyWeight,
                    item.Day
                ];
                foreach (var c in item.CorrectedRouteIntakeRecords) {
                    row.Add(c.Exposure.ToString("G3"));
                }
                row.Add(item.NonDietaryIntakePerBodyWeight.ToString("G3"));
                sb.AppendTableRow(row.ToArray());
            }
            sb.Append("</tbody></table>");
        }
    }
}
