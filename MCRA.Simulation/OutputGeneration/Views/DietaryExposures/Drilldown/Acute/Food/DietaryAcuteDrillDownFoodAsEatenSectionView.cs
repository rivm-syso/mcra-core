using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownFoodAsEatenSectionView : SectionView<DrillDownRecordSection<DietaryAcuteDrillDownRecord>> {
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
                "the total consumption is the total amount of all food as eaten consumed that day",
                "the net consumption is the total amount of food as eaten with positive concentration values consumed that day",
                $"cumulative concentration in {referenceCompoundName}{equivalents}",
                $"cumulative exposure per food as eaten = consumption food as eaten * {referenceCompoundName}{equivalents}/ {item.BodyWeight} (= body weight)"
            };

            //Render HTML
            sb.Append("<h4>Per food as eaten</h4>");
            sb.Append("<div class=\"section\">");
            var intakesPerFoodAsEaten = showZeroConcentrations ? item.IntakeSummaryPerFoodAsEatenRecords : item.IntakeSummaryPerFoodAsEatenRecords.Where(i => i.IntakePerMassUnit > 0);
            if (intakesPerFoodAsEaten != null) {
                if (item.IntakeSummaryPerFoodAsEatenRecords.Where(c => c.IntakePerMassUnit > 0).Count() > 1) {
                    var chartCreator = new DietaryAcuteFoodAsEatenPieChartCreator(item);
                    sb.AppendChart(
                        "DietaryAcuteFoodAsEatenPieChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                sb.AppendParagraph($"Intake = consumption food as eaten * {referenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight)");
                sb.Append(TableHelpers.BuildCustomTableLegend(column, description));
                sb.Append("<table class=\"sortable\"><thead>");
                var row = new ArrayList {
                    $"Food as eaten name",
                    $"Food as eaten code",
                    $"Total consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                    $"Net consumption ({ViewBag.GetUnit("ConsumptionUnit")})",
                    $"ReferenceCompoundNameequivalents ({ViewBag.GetUnit("ConcentrationUnit")})",
                    $"Exposure ({ViewBag.GetUnit("IntakeUnit")})"
                };
                sb.AppendHeaderRow(row.ToArray());
                sb.Append("</thead><tbody>");
                foreach (var ipf in intakesPerFoodAsEaten) {
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
                sb.AppendParagraph("No exposures per food as eaten available");
            }
            sb.Append("</div>");
        }
    }
}
