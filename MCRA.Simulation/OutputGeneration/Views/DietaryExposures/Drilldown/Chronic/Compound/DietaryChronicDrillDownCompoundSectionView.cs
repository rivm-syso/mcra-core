using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrillDownCompoundSectionView : SectionView<DrillDownRecordSection<DietaryChronicDrillDownRecord>> {
        public override void RenderSectionHtml(StringBuilder sb) {
            DietaryChronicDrillDownRecord item = Model.DrillDownRecord;
            string referenceCompoundName = Model.ReferenceSubstanceName;
            bool isCumulative = Model.IsCumulative;
            var equivalents = isCumulative ? " equivalents" : " ";
            var column = new List<string>() {
                "Day",
                $"Exposure per person day ({ViewBag.GetUnit("PerPersonIntakeUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")})",
                "RPF",
                $"Exposure {referenceCompoundName.ToHtml()}{equivalents}({ViewBag.GetUnit("IntakeUnit")})"
            };
            var description = new List<string>() {
                "day in survey",
                "per substance: exposure per person day",
                "per substance: exposure (= exposure per person/ body weight)",
                "Relative Potency Factor",
                $"cumulative exposure in {referenceCompoundName.ToHtml()}{equivalents}= exposure * relative potency factor. In the piechart, exposures are aggregated over survey days" ,
            };

            //Render HTML
            sb.Append("<h4>Per substance</h4>");
            sb.Append("<div class=\"section\">");
            var uniqueSubstanceNameCount = item.DayDrillDownRecords
                .SelectMany(dd => dd.DietaryIntakeSummaryPerCompoundRecords)
                .Select(dd => dd.CompoundName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            if (uniqueSubstanceNameCount > 1) {
                var chartCreator = new DietaryChronicCompoundPieChartCreator(item);
                sb.AppendChart(
                    "DietaryChronicCompoundPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
            sb.AppendParagraph($"Exposure per day {referenceCompoundName}{equivalents} = exposure * relative potency factor");
            sb.AppendParagraph($"body weight: {item.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit")}");
            sb.Append(TableHelpers.BuildCustomTableLegend(column, description));

            var row = new ArrayList {
                "Day",
                "Substance name",
                "Substance code",
                $"Exposure per person day ({ViewBag.GetUnit("PerPersonIntakeUnit")})",
                $"Exposure ({ViewBag.GetUnit("IntakeUnit")})"
            };
            if (isCumulative) {
                row.Add("RPF");
                row.Add($"Exposure {referenceCompoundName}{equivalents} ({ViewBag.GetUnit("IntakeUnit")})");
            }

            sb.Append("<table class='sortable'><thead>");
            sb.AppendHeaderRow(row.ToArray());
            sb.Append("</thead><tbody>");
            var counter = 0;
            foreach (var dayDrillDown in item.DayDrillDownRecords) {
                var intakesPerCompound = dayDrillDown.DietaryIntakeSummaryPerCompoundRecords;
                counter = 0;
                if (intakesPerCompound != null) {
                    foreach (var ipc in intakesPerCompound) {
                        row = [
                        dayDrillDown.Day,
                        ipc.CompoundName,
                        ipc.CompoundCode,
                        (item.BodyWeight * ipc.DietaryIntakeAmountPerBodyWeight / ipc.RelativePotencyFactor).ToString("G3"),
                        (ipc.DietaryIntakeAmountPerBodyWeight / ipc.RelativePotencyFactor).ToString("G3")
                    ];
                        if (isCumulative) {
                            row.Add(ipc.RelativePotencyFactor.ToString("G3"));
                            row.Add(ipc.DietaryIntakeAmountPerBodyWeight.ToString("G4"));
                        }
                        sb.AppendTableRow(row.ToArray());
                        counter++;
                        if (counter > Model.DisplayNumber) {
                            break;
                        }
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
