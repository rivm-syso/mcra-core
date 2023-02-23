using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownCompoundSectionView : SectionView<DrillDownRecordSection<DietaryAcuteDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var item = Model.DrillDownRecord;
            var referenceCompoundName = Model.ReferenceSubstanceName.ToHtml();
            var isCumulative = Model.IsCumulative;
            var equivalents = isCumulative ? " equivalents" : " ";
            var column = new List<string>() {
                $"Exposure per person day ({ViewBag.GetUnit("RawIntakeUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")})",
                "RPF",
                $"Exposure {referenceCompoundName}{equivalents}({ViewBag.GetUnit("IntakeUnit")})"
            };
            var description = new List<string>() {
                "per substance: exposure per person day",
                "per substance: exposure  (= exposure per person/ body weight)",
                "Relative Potency Factor",
                $"cumulative exposure in {referenceCompoundName}{equivalents}= exposure * relative potency factor"
            };

            sb.Append("<h4>Per substance</h4>");
            sb.Append("<div class=\"section\">");
            if (item.DietaryIntakePerMassUnit > 0) {
                if (item.IntakeSummaryPerCompoundRecords.Count() > 1 && item.IntakeSummaryPerCompoundRecords.Where(c => c.DietaryIntakeAmountPerBodyWeight > 0).Count() > 1) {
                    var chartCreator = new DietaryAcuteCompoundPieChartCreator(item);
                    sb.AppendChart(
                        "DietaryAcuteCompoundPieChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            } else {
                sb.AppendParagraph("No positive exposure");
            }
            sb.AppendParagraph($"Exposure {referenceCompoundName}{equivalents} = exposure * relative potency factor");
            sb.AppendParagraph($"body weight: {item.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit")}");
            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));
            sb.Append("<table class=\"sortable\"><thead>");
            //build header row
            var row = new ArrayList {
                $"Substance name",
                $"Substance code",
                $"Exposure per person day ({ViewBag.GetUnit("RawIntakeUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")})"
            };
            if (isCumulative) {
                row.Add($"RPF");
                row.Add($"Exposure {referenceCompoundName}{equivalents} ({ViewBag.GetUnit("IntakeUnit")})");
            }
            sb.AppendHeaderRow(row.ToArray());
            sb.Append("</thead><tbody>");

            foreach (var ipc in item.IntakeSummaryPerCompoundRecords) {
                row = new ArrayList {
                    ipc.CompoundName,
                    ipc.CompoundCode,
                    (item.BodyWeight * ipc.DietaryIntakeAmountPerBodyWeight / ipc.RelativePotencyFactor).ToString("G3"),
                    (ipc.DietaryIntakeAmountPerBodyWeight / ipc.RelativePotencyFactor).ToString("G3")
                };
                if (isCumulative) {
                    row.Add(ipc.RelativePotencyFactor.ToString("G3"));
                    row.Add(ipc.DietaryIntakeAmountPerBodyWeight.ToString("G3"));
                }
                sb.AppendTableRow(row.ToArray());
            }
            sb.Append("</tbody></table>");
            sb.Append("</div>");
        }
    }
}
