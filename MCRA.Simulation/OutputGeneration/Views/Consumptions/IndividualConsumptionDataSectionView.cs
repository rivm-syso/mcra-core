using MCRA.Simulation.OutputGeneration.Helpers;
using System.Globalization;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualConsumptionDataSectionView : SectionView<IndividualConsumptionDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.Append("<p class=\"description\">");
            sb.Append($"Survey: {Model.Survey}");
            sb.Append("</p>");

            if ((Model.SelectedPropertyRecords?.Count > 0) && Model.PopulationSubsetSelection) {
                sb.AppendTable(
                    Model,
                    Model.SelectedPropertyRecords,
                    "SelectedPropertiesTable",
                    ViewBag,
                    caption: "Selected population properties and levels.",
                    saveCsv: true,
                    header: true
                );
            } else {
                sb.AppendDescriptionParagraph($"No population properties selected (full population)");
            }

            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "PopulationCharacteristicsDataTable",
                    ViewBag,
                    caption: "Survey individuals statistics.",
                    saveCsv: true,
                    header: true
                );
            }

            var column = new List<string>() {
                "Individuals",
                "Individual-days",
                "Consumption-days",
                "Survey individuals statistics"
            };
            var description = new List<string>() {
                "Number of individuals in the (selected) population, sum of sampling weights (only if weights unequal to 1).",
                "Number of individual-days, sum of sampling weights (only if weights unequal to 1).",
                "Number of days with one or more consumptions that contribute to the exposure, sum of sampling weights (only if weights unequal to 1).",
                "For covariables: the minimum and maximum value and number of distinct values. For cofactors, the labels and number of values per label."
            };

            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));
            sb.Append("<table>");
            sb.Append("<caption>Individual consumption days.</caption>");
            sb.Append("<thead><tr><th></th>");
            sb.Append("<th>Count</th>");
            if (Model.UseSamplingWeights) {
                sb.Append("<th>Sum of weights</th>");
            }
            sb.Append("</tr></thead>");
            sb.Append("<tbody><tr>");
            sb.Append("<td>Individuals</td>");
            sb.Append($"<td>{Model.TotalIndividuals}</td>");
            if (Model.UseSamplingWeights) {
                sb.Append($"<td>{Model.TotalSamplingWeights.ToString("F1", CultureInfo.InvariantCulture)}</td>");
            }
            sb.Append("</tr><tr>");
            sb.Append("<td>Individual days</td>");
            sb.Append($"<td>{Model.TotalIndividualDays}</td>");
            if (Model.UseSamplingWeights) {
                sb.Append($"<td>{Model.TotalSamplingWeightsPerDay.ToString("F1", CultureInfo.InvariantCulture)}</td>");
            }
            sb.Append("</tr><tr>");
            sb.Append("<td>Consumption-days</td>");
            sb.Append($"<td>{Model.TotalIndividualUserDays} ({$"{100 * (double)Model.TotalIndividualUserDays / Model.TotalIndividualDays:F1}"} % of total)</td>");
            if (Model.UseSamplingWeights) {
                sb.Append($"<td>{Model.TotalSamplingWeightsPerUserDay.ToString("F1")} ({$"{100 * Model.TotalSamplingWeightsPerUserDay / Model.TotalSamplingWeightsPerDay:F1}"} % of total)</td>");
            }
            sb.Append("</tr></tbody></table>");

            if (Model.DateTimeMonthRecords?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.DateTimeMonthRecords,
                    "IndividualDayCharacteristicsDataTable",
                    ViewBag,
                    caption: "Individual day statistics.",
                    saveCsv: true,
                    header: true
                );
            }
        }
    }
}
