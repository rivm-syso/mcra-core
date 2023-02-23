using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrillDownFoodAsEatenSectionView : SectionView<DrillDownRecordSection<DietaryChronicDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            DietaryChronicDrillDownRecord item = Model.DrillDownRecord;
            string referenceCompoundName = Model.ReferenceSubstanceName.ToHtml();
            bool isCumulative = Model.IsCumulative;
            var equivalents = isCumulative ? " equivalents" : " ";
            var column = new List<string>() {
                "Day",
                "Consumption (" + ViewBag.GetUnit("ConsumptionUnit") + ")",
                $"{referenceCompoundName}{equivalents}({ViewBag.GetUnit("ConcentrationUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")}) "
            };
            var description = new List<string>() {
                "consumption day",
                "the total amount of food as eaten with positive concentration values consumed that day",
                $"cumulative concentration in {referenceCompoundName}{equivalents}",
                $"cumulative exposure per food as eaten = consumption food as eaten * {referenceCompoundName}{equivalents}/ {item.BodyWeight} (= body weight). In the piechart, exposures are aggregated over survey days"
            };

            //Render HTML
            sb.Append("<h3>Per food as eaten</h3>");
            sb.Append("<div class=\"section\">");
            if (Model.DrillDownRecord.DayDrillDownRecords.SelectMany(dd => dd.IntakeSummaryPerFoodAsEatenRecords).GroupBy(dd => dd.FoodName).Count() > 1) {
                var chartCreator = new DietaryChronicFoodAsEatenPieChartCreator(item);
                sb.AppendChart(
                    "DietaryChronicFoodAsEatenPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
            sb.AppendParagraph($"Exposure per day= consumption food as eaten * {referenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight).");
            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));

            sb.Append("<table class='sortable'><thead>");
            sb.AppendHeaderRow(
                "Day",
                "Food as eaten name",
                "Food as eaten code",
                $"Consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                $"{referenceCompoundName}{equivalents} ({ViewBag.GetUnit("ConcentrationUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")})"
            );
            sb.Append("</thead><tbody>");
            var counter = 0;
            foreach (var dayDrillDown in item.DayDrillDownRecords) {
                var intakesPerFoodAsEaten = dayDrillDown.IntakeSummaryPerFoodAsEatenRecords;
                counter = 0;
                foreach (var ipf in intakesPerFoodAsEaten) {
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
