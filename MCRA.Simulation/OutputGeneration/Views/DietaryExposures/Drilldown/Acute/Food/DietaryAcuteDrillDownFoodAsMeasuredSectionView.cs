using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownFoodAsMeasuredSectionView : SectionView<DrillDownRecordSection<DietaryAcuteDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var item = Model.DrillDownRecord;
            var showZeroConcentrations = false;
            var referenceCompoundName = Model.ReferenceSubstanceName.ToHtml();
            var isCumulative = Model.IsCumulative;
            var equivalents = isCumulative ? " equivalents" : " ";
            var column = new List<string>() {
                $"Total consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                $"Net consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                $"{referenceCompoundName}{equivalents}({ViewBag.GetUnit("ConcentrationUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")}) "
            };
            var description = new List<string>() {
                "the total consumption is the total amount of all modelled food consumed that day",
                "the net consumption is the total amount of modelled food with positive concentration values consumed that day",
                $"cumulative concentration in {referenceCompoundName}{equivalents}",
                $"cumulative exposure per modelled food = consumption modelled food * {referenceCompoundName}{equivalents}/ {item.BodyWeight} (= body weight)"
            };

            //Render HTML
            sb.Append("<h4>Per modelled food</h4>");
            sb.Append("<div class=\"section\">");
            var intakesPerFoodAsMeasured = showZeroConcentrations ? item.IntakeSummaryPerFoodAsMeasuredRecords : item.IntakeSummaryPerFoodAsMeasuredRecords.Where(i => i.Concentration > 0);
            if (intakesPerFoodAsMeasured != null) {
                if (item.IntakeSummaryPerFoodAsMeasuredRecords.Where(c => c.IntakePerMassUnit > 0).Count() > 1) {
                    var chartCreator = new DietaryAcuteFoodAsMeasuredPieChartCreator(item);
                    sb.AppendChart(
                        "DietaryAcuteFoodAsMeasuredPieChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                sb.AppendParagraph($"Intake = consumption modelled food * {referenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight)");
                sb.Append(TableHelpers.BuildCustomTableLegend(column, description));
                sb.Append("<table class=\"sortable\"><thead>");
                var row = new ArrayList {
                    $"Modelled food name",
                    $"Modelled food code",
                    $"Total consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                    $"Net consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                    $"{referenceCompoundName}{equivalents} ({ViewBag.GetUnit("ConcentrationUnit")})",
                    $"Exposure ({ViewBag.GetUnit("IntakeUnit")})"
                };
                sb.AppendHeaderRow(row.ToArray());
                sb.Append("</thead><tbody>");
                foreach (var ipf in intakesPerFoodAsMeasured) {
                    row = new ArrayList {
                        ipf.FoodName,
                        ipf.FoodCode,
                        double.IsNaN(ipf.GrossAmountConsumed) ? "-" : ipf.GrossAmountConsumed.ToString("G3"),
                        double.IsNaN(ipf.AmountConsumed) ? "-" : ipf.AmountConsumed.ToString("G3"),
                        double.IsNaN(ipf.Concentration) ? "-" : ipf.Concentration.ToString("G4"),
                        double.IsNaN(ipf.IntakePerMassUnit) ? "-" : ipf.IntakePerMassUnit.ToString("G3")
                    };
                    sb.AppendTableRow(row.ToArray());
                }
                sb.Append("</tbody></table>");
            } else {
                sb.AppendParagraph("No exposures per modelled food available");
            }
            sb.Append("</div>");
        }
    }
}
