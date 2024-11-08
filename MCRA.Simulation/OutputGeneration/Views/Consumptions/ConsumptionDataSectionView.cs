using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumptionDataSectionView : SectionView<ConsumptionDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.ConsumptionFrequencies?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.ConsumptionFrequencies,
                    "ConsumptionFrequenciesDataTable",
                    ViewBag,
                    caption: "Consumption frequencies.",
                    saveCsv: true,
                    header: true
                );
            }

            var column = new List<string>() {
                "Number of foods",
                "Number of consumptions",
                "Average number of consumptions per day",
            };
            var description = new List<string>() {
                "the total number of food as eaten/modelled food",
                "the total number of consumptions per food as eaten/modelled food",
                "the average number of consumptions per food as eaten, modelled food",
            };

            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));
            sb.Append(@"<table>
                <caption>Consumptions</caption>
                <thead><tr>
                  <th></th>
                  <th>Number of foods</th>
                  <th>Number of consumptions</th>
                  <th>Average number of consumptions per day</th>
                </tr></thead>");

            sb.Append("<tbody><tr>");
            sb.Append("<td>Food as eaten</td>");
            sb.Append($"<td>{Model.NumberOfFoodAsEaten}</td>");
            sb.Append($"<td>{Model.TotalConsumptionsFoodAsEaten}</td>");
            sb.Append($"<td>{$"{(double)Model.TotalConsumptionsFoodAsEaten / Model.TotalIndividualDays:G3}"}</td>");
            sb.Append("</tr>");
            if (!double.IsNaN(Model.NumberOfFoodAsMeasured)) {
                sb.Append("<tr>");
                sb.Append("<td>Modelled food</td>");
                sb.Append($"<td>{Model.NumberOfFoodAsMeasured}</td>");
                sb.Append($"<td>{Model.TotalConsumptionsFoodAsMeasured}</td>");
                sb.Append($"<td>{$"{(double)Model.TotalConsumptionsFoodAsMeasured / Model.TotalIndividualDays:G3}"}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
        }
    }
}
