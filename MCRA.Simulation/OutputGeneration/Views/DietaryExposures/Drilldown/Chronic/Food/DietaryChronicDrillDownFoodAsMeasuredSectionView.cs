using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrillDownFoodAsMeasuredSectionView : SectionView<DrillDownRecordSection<DietaryChronicDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            DietaryChronicDrillDownRecord item = Model.DrillDownRecord;
            string referenceCompoundName = Model.ReferenceSubstanceName.ToHtml();
            bool isCumulative = Model.IsCumulative;
            var equivalents = isCumulative ? " equivalents" : " ";
            var column = new List<string>() {
                "Day",
                $"Consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                $"{referenceCompoundName}{equivalents}({ViewBag.GetUnit("ConcentrationUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")}) "
            };
            var description = new List<string>() {
                "day in survey",
                "the consumption is the total amount of modelled food with positive concentration values consumed that day",
                $"cumulative concentration in {referenceCompoundName}{equivalents}",
                $"cumulative exposure per modelled food = consumption modelled food * {referenceCompoundName}{equivalents}/ {item.BodyWeight} (= body weight). In the piechart, exposures are aggregated over survey days"
            };

            //Render HTML
            sb.Append("<h3>Per modelled food</h3>");
            sb.Append("<div class=\"section\">");

            var uniqueFoodNameCount = Model.DrillDownRecord.DayDrillDownRecords
                .SelectMany(dd => dd.IntakeSummaryPerFoodAsMeasuredRecords)
                .Select(dd => dd.FoodName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            if (uniqueFoodNameCount > 1) {
                var chartCreator = new DietaryChronicFoodAsMeasuredPieChartCreator(item);
                sb.AppendChart(
                    "DietaryChronicFoodAsMeasuredPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
            sb.AppendParagraph($"Exposure per day= consumption modelled food * {referenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight).");
            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));

            sb.Append("<table class='sortable'><thead>");
            sb.AppendHeaderRow(
                "Day",
                "Modelled food name",
                "Modelled food code",
                $"Consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                $"{referenceCompoundName}{equivalents} ({ViewBag.GetUnit("ConcentrationUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")})"
            );
            sb.Append("</thead><tbody>");
            var counter = 0;
            foreach (var dayDrillDown in item.DayDrillDownRecords) {
                var intakesPerFoodAsMeasured = dayDrillDown.IntakeSummaryPerFoodAsMeasuredRecords;
                counter = 0;
                foreach (var ipf in intakesPerFoodAsMeasured) {
                    sb.AppendTableRow(
                        dayDrillDown.Day,
                        ipf.FoodName,
                        ipf.FoodCode,
                        ipf.AmountConsumed.ToString("G3"),
                        ipf.Concentration.ToString("G4"),
                        ipf.IntakePerMassUnit.ToString("G4")
                    );
                    counter++;
                    if (counter > Model.DisplayNumber) {
                        break;
                    }
                }
            }
            sb.Append("</tbody></table>");
            if (counter > Model.DisplayNumber) {
                sb.Append($"<p>Only the first {Model.DisplayNumber} records are displayed</p>");
            }
            sb.Append("</div>");
        }
    }
}
